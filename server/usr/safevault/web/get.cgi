#!/bin/sh

echo "Content-Type: text/json"
echo ""

OTPBIN=/usr/safevault
OTPTMP=/tmp/safevault/$HTTPD_PID
if [ "$HTTPD_PID" == "" ]; then
	OTPTMP=/tmp/safevault/0000
fi
mkdir -p $OTPTMP

echo -n "" > $OTPTMP/wquery.txt
if [ "$QUERY_STRING" != "" ]; then
	OIFS="$IFS"
	IFS="${OIFS}&"
	set $QUERY_STRING
	qstring1="$*"
	IFS="$OIFS"

	for i in $qstring1 ;do
		echo "$i" >> $OTPTMP/wquery.txt
	done
fi

debug="";
exception=""

GetHttpRequestParam() 
{
	if [ "$exception" = "" ]; then 
		retVal=$1
		local paramName=$2		
		local value=`/bin/get_key_value $OTPTMP/wquery.txt $paramName`
		if [ "$value" != "" ]; then 
			eval "$retVal='$value'"
		else
			debug="$debug;get($paramName)=false";
			exception="1";
		fi
		
	fi
}
AssertFileContent() {
	if [ "$exception" == "" ]; then 
		local action=$1
		local filename=$2
		if [ -f $filename ]; then
			filesize=`wc -c < "$filename"`
			if [ $filesize == 0 ]; then
				debug="$debug;fail $action";
				exception="1";
			fi
		else
			debug="$debug;fail $action";
			exception="1";
		fi		
	fi
}
GetQueryParam() 
{
	if [ "$exception" == "" ]; then 
		retVal=$1
		local paramName=$2		
		local value=`/bin/get_key_value $OTPTMP/query.txt $paramName`
		if [ "$value" != "" ]; then 
			eval "$retVal='$value'"
		else
			debug="$debug;query($paramName)=false";
			exception="1";
		fi
		
	fi
}

GetAesParam() 
{
	if [ "$exception" == "" ]; then 
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
	fi
}

GetHttpRequestParam data "a" 
GetHttpRequestParam passw "b"

if [ "$exception" == "" ]; then
	
	echo -n "$data" > $OTPTMP/data.b64
	echo -n "$passw" > $OTPTMP/passw.b64
	
	#echo "$QUERY_STRING"
	#echo "data1: $data"
	#echo "passw: $passw"
	
	if [ "$exception" == "" ]; then
		openssl enc -base64 -d -A -in $OTPTMP/passw.b64 -out $OTPTMP/passw.enc 
		AssertFileContent "decode-b64(passw)" "$OTPTMP/passw.enc"
	fi
	
	if [ "$exception" == "" ]; then
		openssl enc -base64 -d -A -in $OTPTMP/data.b64 -out $OTPTMP/data.enc 
		AssertFileContent "decode-b64(data)" "$OTPTMP/data.enc"
	fi
	
	if [ "$exception" == "" ]; then
		openssl rsautl -decrypt -inkey $OTPBIN/server/cer.private.pem -in $OTPTMP/passw.enc -out $OTPTMP/passw.bin
		AssertFileContent "decrypt-rsa(passw; cert-server)" "$OTPTMP/passw.bin"
	fi
	
	if [ "$exception" == "" ]; then
	
		GetAesParam passw_key passw_iv "$OTPTMP/passw.bin"
		openssl enc -d -aes-256-cbc -nosalt -K "$passw_key" -iv "$passw_iv" -in $OTPTMP/data.enc -out $OTPTMP/query.txt
		
		AssertFileContent "decrypt-aes(data; passw)" "$OTPTMP/query.txt"
	fi
	
	if [ "$exception" == "" ]; then
		local value=`/bin/get_key_value $OTPTMP/query.txt username`
		if [ "$value" == "" ]; then 
			debug="$debug;fail decrypt-aes(data)";
			exception="1";
		fi
	fi
	
	#echo "decrypted:";
	#cat $OTPTMP/query.txt
	#echo "end";

else
	debug="$debug;NoEncData";
fi

GetQueryParam username "username"
if [ "$exception" == "" ]; then
	userprofile="$OTPBIN/client/$username/profile.conf"
	if [ ! -f $userprofile ]; then
		debug="$debug;Invalid Username";
		exception="1";
	fi
fi

GetQueryParam password "password"
if [ "$exception" == "" ]; then
	secret=`/bin/get_key_value $userprofile otp-secretkey`
	success=`$OTPBIN/lib/otp match --secret "$secret" --password $password --window 2`
#	success=true;
	if [ "$success" != "true" ]; then
		debug="$debug;Invalid OTP";
		exception="1";	
	fi
fi
	
response=""
if [ "$exception" == "" ]; then

	get_vaultkey=`/bin/get_key_value $OTPTMP/query.txt get-vaultkey`
	if [ "$get_vaultkey" != "" ]; then 
		vault="$OTPBIN/client/$username/vault.conf"
		response=`/bin/get_key_value $vault $get_vaultkey`	
	fi
	
	set_vaultkey=`/bin/get_key_value $OTPTMP/query.txt set-vaultkey`
	if [ "$set_vaultkey" != "" ]; then 
		value=`/bin/get_key_value $OTPTMP/query.txt set-vaultvalue`
		echo "$set_vaultkey=$value" >> "$OTPBIN/client/$username/vault.conf"
		response="OK"	
	fi
fi	

if [ "$exception" == "" ]; then
	GetQueryParam responsepwd "responsepwd"
	echo -n "$responsepwd" > $OTPTMP/responsepwd.b64
	openssl enc -base64 -d -A -in $OTPTMP/responsepwd.b64 -out $OTPTMP/responsepwd.bin
	AssertFileContent "decode-b64(responsepwd)" "$OTPTMP/responsepwd.bin"	
fi

# Encrypt Response
if [ "$exception" == "" ]; then
	echo -n "$response" > $OTPTMP/response1.bin
	openssl rand 32 > $OTPTMP/passw1.bin
	
	GetAesParam passw_key passw_iv "$OTPTMP/passw1.bin"
	GetAesParam response_key response_iv "$OTPTMP/responsepwd.bin"
	
	#-- Encrypt response
	if [ "$exception" == "" ]; then
		openssl enc -aes-256-cbc -K "$passw_key" -iv "$passw_iv" -in $OTPTMP/response1.bin -out $OTPTMP/response1a.enc
		AssertFileContent "encrypt-aes(response; passw)" "$OTPTMP/response1a.enc"
	fi
	if [ "$exception" == "" ]; then
		openssl enc -aes-256-cbc -K "$response_key" -iv "$response_iv" -in $OTPTMP/response1a.enc -out $OTPTMP/response1.enc
		AssertFileContent "encrypt-aes(response; responsepwd)" "$OTPTMP/response1.enc"
	fi
	
	#-- Encrypt Password
	if [ "$exception" == "" ]; then
		openssl enc -aes-256-cbc -K "$response_key" -iv "$response_iv" -in $OTPTMP/passw1.bin -out $OTPTMP/passw1a.enc
		AssertFileContent "encrypt-aes(password; responsepwd)" "$OTPTMP/passw1a.enc"
	fi
	if [ "$exception" == "" ]; then
		openssl rsautl -encrypt -pubin -inkey $OTPBIN/client/$username/cer.pub.pem -in $OTPTMP/passw1a.enc -out $OTPTMP/passw1.enc
		AssertFileContent "encrypt-rsa(password; cert-client)" "$OTPTMP/passw1.enc"
	fi
	
	#-- Encode Base64
	if [ "$exception" == "" ]; then
		openssl enc -base64 -A -in $OTPTMP/passw1.enc -out $OTPTMP/passw1.b64 
		AssertFileContent "encode-b64(password)" "$OTPTMP/passw1.b64"
	fi
	if [ "$exception" == "" ]; then
		openssl enc -base64 -A -in $OTPTMP/response1.enc -out $OTPTMP/response1.b64
		AssertFileContent "encode-b64(response)" "$OTPTMP/response1.b64"
	fi
	debug="$debug;OK";
fi

if [ "$exception" != "" ]; then
	openssl rand 256 > $OTPTMP/response1.bin
	openssl rand 32 > $OTPTMP/passw1.bin
	
	openssl enc -base64 -A -in $OTPTMP/passw1.bin -out $OTPTMP/passw1.b64
	openssl enc -base64 -A -in $OTPTMP/response1.bin -out $OTPTMP/response1.b64
fi

echo "$QUERY_STRING" >> $PWD/log/request.log
echo "$debug" >> $PWD/log/request.log

response=`cat $OTPTMP/response1.b64`
passwd=`cat $OTPTMP/passw1.b64`

rm -rf $OTPTMP;

utc=`date -u +%Y-%m-%dT%H:%M:%S`

echo "{ \"utc\":\"$utc\", \"a\":\"$response\", \"b\":\"$passwd\" }"
#echo "{ \"utc\":\"$utc\", \"a\":\"$response\", \"b\":\"$passwd\", \"debug\":\"$debug\" }"
#echo "-"
#/usr/bin/env

exit