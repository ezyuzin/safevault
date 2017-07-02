#!/bin/sh

OTPBIN=/usr/safevault

DEBUG="0";
DEBUG_LOG=false;
BLACKLIST_ENABLED=true;

LASTEXITCODE=0;
HTTPSTATUS="200";
RESPONSE_STATUS="OK"

utc=`date -u +%Y-%m-%dT%H:%M:%S`

log()
{
	echo "$utc |$remoteIP |$1" >> $OTPBIN/log/access.log
	if [ $DEBUG_LOG == "1" ]; then
		echo "$utc |$remoteIP |$1"	
	fi
}
LogError()
{
	log "ERROR |$1"
}
LogInfo()
{
	log "INFO  |$1"
}
throw() {
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	LASTEXITCODE=1;
	HTTPSTATUS=$1;
	RESPONSE_STATUS=$2;
}

getRemoteIP() {
	remoteIP=$HTTP_X_REAL_IP;
	if [ "$remoteIP" == "" ]; then 
		remoteIP=$REMOTE_ADDR
	fi

	remoteIP=`echo "$remoteIP" | sed -r 's/[^1234567890abcdefghijklmnopqrstuvwxyz]/./g'`
}

setupDebugMode()
{
	if [ $DEBUG != "" ]; then
		getHttpQueryParam DEBUG_LOG "debug"
		if [ $DEBUG_LOG == "1" ]; then
			echo "Content-Type: text/text"
			echo ""
		fi
	fi
}

banlistProcess() {
	if [ $BLACKLIST_ENABLED != "true" ]; then
		return;
	fi
	if [ ! -f "$OTPBIN/var/ban.txt" ]; then
		return;
	fi
	
	value=`/bin/get_key_value $OTPBIN/var/ban.txt $remoteIP`
	if [ "$value" != "" ]; then
		LogError "ClientIP Banned";
		throw "403"
		return;
	fi
}
	
createTmpFolder() {
	OTPTMP=/tmp/safevault/$HTTPD_PID
	if [ "$HTTPD_PID" == "" ]; then
		OTPTMP=/tmp/safevault/0000
	fi
	rm -rf $OTPTMP	
	mkdir -p $OTPTMP
}

criticalFault() {
	
	local faultName=$1
	local points=1
	mkdir -p $OTPBIN/var/ban-log
	find "$OTPBIN/var/ban-log" -mmin +15 -type f -delete	
	
	if [ "$faultName" == "Invalid-OneTimePassword" ]; then 
		points=2
	fi
	if [ "$faultName" == "Invalid-SyncPassword" ]; then 
		points=2
	fi	
	if [ "$faultName" == "Invalid-VaultKey" ]; then 
		points=2
	fi	
	
	local banlog="$OTPBIN/var/ban-log/$remoteIP.txt"
	if [ -f "$banlog" ]; then
		local points1=`/bin/get_key_value $banlog points`
		if [ "$points1" != "" ]; then 
			points=`expr ${points} + ${points1}` 
		fi
	fi
	
	if [ $points -ge 10 ]; then
		echo "$remoteIP=$utc" >> $OTPBIN/var/ban.txt
		rm "$OTPBIN/var/ban-log/$remoteIP.txt"
	else
		echo "points=$points" > $banlog
	fi
}

getHttpQueryParam() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local value=`/bin/get_key_value $OTPTMP/wquery.txt $2`
	eval "$1='$value'"
}

getQueryParam() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local value=`/bin/get_key_value $OTPTMP/query.txt $2`
	eval "$1='$value'"
}

assertFileState() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local filename=$1
	local action=$2	
	local httpCode=$3
	
	if [ ! -f $filename ]; then
		LogError "$action: File '$filename' not found";
		throw "$httpCode" "$action"
		return;
	fi
	
	filesize=`wc -c < "$filename"`
	if [ $filesize == 0 ]; then
		LogError "$action: File '$filename' empty"
		throw "$httpCode" "$action"
		return;
	fi
}

getAesParam() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local filename=$3		

	openssl dgst -binary -md5 $filename > $OTPTMP/hash1.bin
	cat $filename >> $OTPTMP/hash1.bin
	openssl dgst -binary -md5 $OTPTMP/hash1.bin > $OTPTMP/hash2.bin
	cat $filename >> $OTPTMP/hash2.bin		
	
	hash1=`openssl dgst -md5 < $filename | sed 's/^.* //'`
	hash2=`openssl dgst -md5 < $OTPTMP/hash1.bin | sed 's/^.* //'`
	hash3=`openssl dgst -md5 < $OTPTMP/hash2.bin | sed 's/^.* //'`
	
	eval "$1='$hash1$hash2'"
	eval "$2='$hash3'"
}

readQuery()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	readBinaryRequest $OTPTMP/query.txt
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local value=`/bin/get_key_value $OTPTMP/query.txt username`
	if [ "$value" == "" ]; then 
		LogError "Invalid request. Decrypt failed"
		throw "400"
		rm $OTPTMP/query.txt
		return;
	fi
	
	if [ $DEBUG_LOG != "" ]; then
		cat $OTPTMP/query.txt	
		echo
		echo
	fi	
}

readBinaryPos=0;
readBinaryRequest()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	outputFile=$1;
	
	if [ ! -f $OTPTMP/requestPwd.enc ]; then
		readBinaryPos=`$OTPBIN/lib/fchunk read -seek $readBinaryPos -in "$OTPTMP/request.bin" -out "$OTPTMP/requestPwd.enc"`
		if [ ! -f "$OTPTMP/requestPwd.enc" ]; then 
			LogError "Message password not found"
			throw "400"	
			return;
		fi
		
		openssl rsautl -decrypt -inkey $OTPBIN/server/cer.private.pem -in $OTPTMP/requestPwd.enc -out $OTPTMP/requestPwd.bin
		if [ ! -f "$OTPTMP/requestPwd.bin" ]; then 
			LogError "Invalid message password. Decrypt failed"
			throw "400"	
			return;
		fi		
	fi
	
	readBinaryPos=`$OTPBIN/lib/fchunk read -seek $readBinaryPos -in "$OTPTMP/request.bin" -out "$OTPTMP/data.enc"`
	if [ ! -f "$OTPTMP/data.enc" ]; then 
		LogError "Message data chunk not found"
		throw "400"	
		return;
	fi	

	getAesParam passw_key passw_iv "$OTPTMP/requestPwd.bin"
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	
	
	openssl enc -d -aes-256-cbc -nosalt -K "$passw_key" -iv "$passw_iv" -in $OTPTMP/data.enc -out $OTPTMP/data.bin
	if [ ! -f $OTPTMP/data.bin ]; then 
		LogError "Invalid data. Decrypt failed"
		throw "400"	
		return;
	fi
	cp $OTPTMP/data.bin $outputFile

}

getQueryPassword()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi

	local _password="";
	getQueryParam _password "password"
	if [ "$_password" == "" ]; then
		LogError "Argument 'password' not found"
		throw "400";
		return;
	fi	
	eval "$1='$_password'"
}

getQueryUsername() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi

	local _username=""
	getQueryParam _username "username"
	if [ "$_username" == "" ]; then
		LogError "Argument 'username' not found"
		throw "400";		
		return;
	fi
	
	local _username1=`echo "$_username" | sed -r 's/[^1234567890abcdefghijklmnopqrstuvwxyz]/./g'`
	if [ "$_username1" != "$_username" ]; then
	
		LogError "Username contains unallowed symbols ($_username)"
		throw "400"
		return;
	fi
	
	local userprofile="$OTPBIN/client/$_username/profile.conf"
	if [ ! -f $userprofile ]; then
		LogError "Username not found ($_username)"
		throw "401"
		return;
	fi

	eval "$1='$_username'"
}

getResponsePwd()
{
	if [ $HTTPSTATUS == "200" ]; then
		LASTEXITCODE=0;
		getQueryParam responsepwd "responsepwd"
		if [ "$responsepwd" != "" ]; then 
			echo -n "$responsepwd" > $OTPTMP/responsepwd.b64
			openssl enc -base64 -d -A -in $OTPTMP/responsepwd.b64 -out $OTPTMP/responsepwd.bin
			assertFileState "$OTPTMP/responsepwd.bin" "Decode-Base64(responsepwd)" "400 Bad Request"
		else
			LogError "Argument 'responsepwd' not found" 
			throw "400"
		fi
	fi
}

createResponsePassword() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	if [ -f $OTPTMP/responsePwd.bin ] && [ -f $OTPTMP/response.bin ]; then
		return;
	fi
	
	if [ ! -f "$OTPBIN/client/$username/cer.pub.pem" ]; then
		logEror "client certificate not found: $OTPBIN/client/$username/cer.pub.pem";
		throw 400;
		return;
	fi	
	
	openssl rand 32 > $OTPTMP/responsePwd.bin
	openssl rsautl -encrypt -pubin -inkey $OTPBIN/client/$username/cer.pub.pem -in $OTPTMP/responsePwd.bin -out $OTPTMP/responsePwd.enc
	if [ ! -f "$OTPTMP/responsePwd.enc" ]; then 
		LogError "Unable encrypt response password with user certiticate"
		throw "400";
	fi
	
	$OTPBIN/lib/fchunk create -in $OTPTMP/responsePwd.enc -out $OTPTMP/response.bin
}

createJsonResponse() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi

	echo -n "{\"utc\":\"$utc\", \"status\":\"$RESPONSE_STATUS\", \"data\":$1 }" > $OTPTMP/response.json	
	createBinaryResponse $OTPTMP/response.json
}

createBinaryResponse() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi

	LogInfo "createBinaryResponse $1"
	filename=$1;
	if [ ! -f $filename ]; then
		LogInfo "binary file not found: $filename"
		throw "500";
		return;
	fi
	
	createResponsePassword;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	#-- encrypt using requestPwd
	getAesParam passw_key passw_iv "$OTPTMP/requestPwd.bin"
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	
	openssl enc -aes-256-cbc -K "$passw_key" -iv "$passw_iv" -in $filename -out $OTPTMP/response_data1.enc
	if [ ! -f "$OTPTMP/response_data1.enc" ]; then 
		LogError "Unable encrypt response data using request password"
		throw "400";
		return;
	fi
	
	#-- encrypt using responsePwd
	getAesParam response_key response_iv "$OTPTMP/responsePwd.bin"
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	openssl enc -aes-256-cbc -K "$response_key" -iv "$response_iv" -in $OTPTMP/response_data1.enc -out $OTPTMP/response_data.enc
	if [ ! -f "$OTPTMP/response_data.enc" ]; then 
		LogError "Unable encrypt response data using response password"
		throw "400";
		return;
	fi	
	
	$OTPBIN/lib/fchunk create -in $OTPTMP/response_data.enc -out $OTPTMP/response.bin
}

checkOneTimePassword() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	local username=$1
	local password=$2
	
	if [ "$DEBUG" == "1" ]; then 
		if [ "$password" == "000000" ]; then 	
			return;
		fi
	fi
	
	secret=`/bin/get_key_value "$OTPBIN/client/$username/profile.conf" otp-secretkey`
	success=`$OTPBIN/lib/otp match --secret "$secret" --password $password --window 2`
	# success="true"
	if [ "$success" != "true" ]; then
		criticalFault "InvalidPassword";
		LogError "Invalid one-time-password($password) for username($username)"
		throw "200" "Invalid-OneTimePassword"  
		return;
	fi
}
checkSyncPassword() {
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	local username=$1
	local password=$2
	
	syncpassw1=`/bin/get_key_value "$OTPBIN/client/$username/profile.conf" syncpasw`
	if [ $DEBUG == "1" ]; then
		if [ "$password" == "000000" ]; then
			syncpassw1="$password";
		fi
	fi
	if [ "$syncpassw1" != "$password" ]; then
		LogError "Invalid syncpasw($password) for username($username)"
		criticalFault "Invalid-SyncPassword";
		throw "200" "Invalid-SyncPassword"
		return;
	fi	
}

checkToken()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	tokenId=`/bin/get_key_value $OTPTMP/query.txt token`
	if [ "$tokenId" == "" ]; then
		LogError "Argument 'token' not found"
		throw "400"
		return;
	fi
	
	mkdir -p "/tmp/safevault/token"
	find "/tmp/safevault/token" -mmin +5 -type f -delete
	
	if [ ! -f "/tmp/safevault/token/$tokenId.bin" ]; then
		LogError "Invalid token"
		throw "400"
		return;
	fi	
	rm "/tmp/safevault/token/$tokenId.bin"
}

cmdGetToken()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	getQueryUsername username;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	LogInfo "get-token($username)"
	
	mkdir -p "/tmp/safevault/token"
	find "/tmp/safevault/token" -mmin +5 -type f -delete
	
	tokenId=`openssl rand 8 -hex`
	tokenFilename="/tmp/safevault/token/$tokenId.bin"
	echo "1" > $tokenFilename
	
	createJsonResponse "\"$tokenId\"";	
}

cmdGetQueryVaultKey()
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	vaultkey=`/bin/get_key_value $OTPTMP/query.txt vault-key`
	if [ "$vaultkey" == "" ]; then 
		LogError "Argument 'vault-key' not found"
		throw "400"
		return
	fi
	LogInfo "get-vaultkey($vaultkey)"
	
	getQueryUsername username;
	getQueryPassword password;
	
	checkOneTimePassword "$username" "$password";
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	local vault="$OTPBIN/client/$username/vault.conf"
	local value=`/bin/get_key_value $vault $vaultkey`
	if [ "$value" == "" ]; then 
		LogError "get-vaultkey($vaultkey) not found for username($username)";
		criticalFault "Invalid-VaultKey";
		throw "200" "Invalid-VaultKey"
		return
	fi
	
	LogInfo "get-vaultkey($vaultkey) for username($username)";
	createJsonResponse "\"$value\"";
}

cmdSetQueryVaultKey() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	vaultkey=`/bin/get_key_value $OTPTMP/query.txt vault-key`
	if [ "$vaultkey" == "" ]; then
		LogError "Argument 'vault-key' not found"
		throw "400"
		return;
	fi
	
	getQueryUsername username;
	getQueryPassword password;
	checkOneTimePassword "$username" "$password";
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	value=`/bin/get_key_value $OTPTMP/query.txt vaultkey-value`
	if [ "$value" == "" ]; then
		LogError "Trying set empty vaultkey($set_vaultkey) for username($username)";
		throw "400"
		return;
	fi

	echo "$vaultkey=$value" >> "$OTPBIN/client/$username/vault.conf"
	LogInfo "set-vaultkey($vaultkey)='$value' for username($username)"
	createJsonResponse "\"OK\"";
}

cmdUpload() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	getQueryUsername username;
	getQueryPassword password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	uuid=`/bin/get_key_value $OTPTMP/query.txt uuid`
	if [ "$uuid" == "" ]; then
		LogError "Argument 'uuid' not found"
		throw "400"
		return;
	fi
	
	md5=`/bin/get_key_value $OTPTMP/query.txt md5`
	if [ "$md5" == "" ]; then
		LogError "Argument 'md5' not found"
		throw "400"
		return;
	fi	

	lastModified=`/bin/get_key_value $OTPTMP/query.txt last-modified`
	if [ "$lastModified" == "" ]; then
		LogError "Argument 'lastModified' not found"
		throw "400"
		return;
	fi	
	
	checkSyncPassword $username $password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	readBinaryRequest "$OTPTMP/$uuid.dbx"
	
	hashMD5=`openssl dgst -md5 < "$OTPTMP/$uuid.dbx" | sed 's/^.* //'`
	if [ $md5 != $hashMD5 ]; then 
		LogError "Invalid MD5 $md5 <> $hashMD5"
		throw "200" "BadMD5"
		return;
	fi
	
	mkdir -p "$OTPBIN/client/$username/dbx"
	
	
	if [ -f "$OTPBIN/client/$username/dbx/$uuid.dbx" ]; then
		mkdir -p "$OTPBIN/client/$username/dbx/bak"
		i=0
		while [ -f "$OTPBIN/client/$username/dbx/bak/$uuid.$i.dbx" ]; do
			i=`expr ${i} + 1`
		done		
		mv "$OTPBIN/client/$username/dbx/$uuid.dbx" "$OTPBIN/client/$username/dbx/bak/$uuid.$i.dbx"
	fi
	
	mv "$OTPTMP/$uuid.dbx" "$OTPBIN/client/$username/dbx/$uuid.dbx"
	echo "$lastModified" > "$OTPBIN/client/$username/dbx/$uuid.inf"
	LogInfo "upload-dbx($uuid), username($username)"
	
	createJsonResponse "\"OK\"";
}

cmdDownload() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	getQueryUsername username;
	getQueryPassword password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	uuid=`/bin/get_key_value $OTPTMP/query.txt uuid`
	if [ "$uuid" == "" ]; then
		LogError "Argument 'uuid' not found"
		throw "400"
		return;
	fi		
	
	checkSyncPassword $username $password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	LogInfo "download($uuid), username($username)"
	if [ ! -f "$OTPBIN/client/$username/dbx/$uuid.dbx" ]; then
		LogInfo "File not found $OTPBIN/client/$username/dbx/$uuid.dbx"
		throw "404"
		return;
	fi	
	
	md5=`openssl dgst -md5 < "$OTPBIN/client/$username/dbx/$uuid.dbx" | sed 's/^.* //'`
	
	dbxFile="$OTPBIN/client/$username/dbx/$uuid.dbx"
	LogInfo "cmdDownload file:$dbxFile"
	
	createJsonResponse "{ \"md5\":\"$md5\" }";
	createBinaryResponse "$dbxFile"	
}

cmdGetLastModified() 
{
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	getQueryUsername username;
	getQueryPassword password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi
	
	uuid=`/bin/get_key_value $OTPTMP/query.txt uuid`
	if [ "$uuid" == "" ]; then
		LogError "Argument 'uuid' not found"
		throw "400"
		return;
	fi		
	
	checkSyncPassword $username $password;
	if [ $LASTEXITCODE != 0 ]; then
		return;
	fi	

	LogInfo "md5($uuid), username($username)"
	if [ ! -f "$OTPBIN/client/$username/dbx/$uuid.inf" ]; then
		LogInfo "File not found $OTPBIN/client/$username/dbx/$uuid.inf"
		throw "404"
		return;
	fi	
	response=`cat $OTPBIN/client/$username/dbx/$uuid.inf`
	createJsonResponse "\"$response\"";
}

getRemoteIP;
createTmpFolder;
setupDebugMode;
banlistProcess;

cat > "$OTPTMP/request.bin"
readQuery;

if [ $LASTEXITCODE == 0 ]; then
	getQueryParam cmd "cmd";

	case "$cmd" in
		get-token)
			cmdGetToken;
		;;
		get-vaultkey)
			checkToken;
			cmdGetQueryVaultKey;
		;;
		set-vaultkey)
			checkToken;
			cmdSetQueryVaultKey;
		;;
		dbx-download)
			checkToken;
			cmdDownload;
		;;
		dbx-getLastModified)
			checkToken;
			cmdGetLastModified;
		;;		
		dbx-upload)
			checkToken;
			cmdUpload;
		;;	
		*)
		LogError "Requested unknown command '$cmd'"
		throw "400" "Unknown Cmd"
	esac
fi

if [ $LASTEXITCODE != 0 ] && [ $HTTPSTATUS == 200 ]; then
	rm $OTPTMP/response.bin
	LASTEXITCODE=0;
	createJsonResponse "\"$RESPONSE_STATUS\"";
fi

if [ $LASTEXITCODE != 0 ]; then
	echo "Content-Type: text/text"
	echo "Status: $HTTPSTATUS"
	echo ""
	echo ""
else
	filesize=`wc -c < "$OTPTMP/response.bin"`
	echo "Content-Type: application/x-www-form-urlencoded"
	echo "Status: $HTTPSTATUS"
	echo "Content-Length: $filesize"
	echo ""
	cat "$OTPTMP/response.bin"	
fi

rm -rf $OTPTMP;
