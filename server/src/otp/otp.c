#include <stdlib.h>
#include <stdio.h>
#include <time.h>
#include <string.h>
#include <math.h>

#include "otp.h"
#include "crypto/hmac.h"
#include "crypto/base32.h"

int main( int argc, const char** argv)
{
	if (argc == 1) { 
		return 0; 
	}

	if (strcmp(argv[1], "encode-base32") == 0) {
		encode_base32(argc, argv);
		return 0;	
	}

	if (strcmp(argv[1], "get") == 0) {
		get_otp(argc, argv);
		return 0;	
	}
	if (strcmp(argv[1], "match") == 0) {
		match_otp(argc, argv);
		return 0;	
	}
}


int match_otp(int argc, const char** argv) 
{
	unsigned char* secret = get_secret(argc, argv);
	if (secret == NULL) {
		return -1;
	}
	int password = get_password(argc, argv);
	if (password == -1) {
		return -1;
	}

	long time = get_time(argc, argv);
	int digits = get_digits(argc, argv);
	int window = get_window(argc, argv);
	short found = 0;

	

	for(int offset = 0; offset <= window; offset++) {
			
		int password1 = calculate_otp(secret, time - offset, digits);
		//printf("%d <> %d\n", password, password1);
		if (password1 == password) {
			found = 1;
			break;
		}
		if (offset != 0) {
			password1 = calculate_otp(secret, time + offset, digits);
			//printf("%d <> %d\n", password, password1);

			if (password1 == password) {
				found = 1;
				break;
			}
		}
	}

	if (found) {
		printf("true\n");
	}
	else {
		printf("false\n");
	}
	free(secret);
}

int get_otp(int argc, const char** argv) 
{
	unsigned char* secret = get_secret(argc, argv);
	if (secret == NULL) {
		return -1;
	}

	long time = get_time(argc, argv);

	int digits = get_digits(argc, argv);

	int code = calculate_otp(secret, time, digits);
	printf("%07d\n", code);
	free(secret);
}


int calculate_otp(const char *secret, long tm, int digits)
{
	unsigned char *data = malloc(sizeof(char) * 8);
	memset(data, 0, sizeof(data));

	for (int i = 8; i != 0; --i)
    {
		long long c = tm&0xFF;
    	data[i-1] = (char) c;
		tm = tm >> 8;
    }

	unsigned char hmac[20];
	int success = hmac_sha1(secret, strlen(secret), data, 8, hmac);
	long code = 0;

	if (success == 0) 
	{
		int offset = hmac[sizeof(hmac)-1]&0xF;
		long truncatedHash = 0;
		
		for (int i = 0; i < 4; ++i)
        {
        	truncatedHash <<= 8;
	        truncatedHash |= (hmac[offset + i] & 0xFF);
		}

		float digits1 = 6;
		long module = pow(10, digits);
		truncatedHash &= 0x7FFFFFFF;
		truncatedHash %= module;

		code = truncatedHash;
	}

	
	free(data); 
	return code;
}


void encode_base32(int argc, const char** argv) {

	if (argc != 3) {
		return;
	}

	char *b32secret = malloc(sizeof(char) * 200);
	const char* secret = argv[2];

	base32_encode(secret, strlen(secret), b32secret);
	printf("%s\n", b32secret);
	free(b32secret);
}

long get_time(int argc, const char** argv) {

	unsigned long timestep = 30;
	const char* s_timestep = get_arg(argc, argv, "--time-step");
	if (s_timestep != NULL) {
		timestep = strtoll(s_timestep, NULL, 0);
	}

	unsigned long tm = (unsigned long)time(NULL);
	tm = tm / timestep;
	return tm;
}

unsigned char* get_secret(int argc, const char** argv) {
	const char* b32secret = get_arg(argc, argv, "--secret");
	if (b32secret == NULL) {
		printf("--secret not found\n");
		return NULL;	
	}

	char *secret = malloc(sizeof(char) * strlen(b32secret));
	memset(secret, 0, strlen(b32secret));

	int secret_len = base32_decode(b32secret, secret);

	return secret;
}

int get_window(int argc, const char** argv) {
	int window = 0;
	const char* s_digits = get_arg(argc, argv, "--window");
	if (s_digits != NULL) {
		window = strtoll(s_digits, NULL, 0);		
	}
	return window;
}

int get_password(int argc, const char** argv) {
	int password = 0;
	const char* s_digits = get_arg(argc, argv, "--password");
	if (s_digits == NULL) {
		printf("--password not found\n");
		return -1;
	}
	password = strtoll(s_digits, NULL, 10);		
	return password;
}

int get_digits(int argc, const char** argv) {
	int digits = 6;
	const char* s_digits = get_arg(argc, argv, "--digits");
	if (s_digits != NULL) {
		digits = strtoll(s_digits, NULL, 0);		
	}
	return digits;
}

const char* get_arg(int argc, const char **argv, const char *param_name) {
	for(int i=0; i< argc; i++) {
		if (strcmp(argv[i], param_name) == 0) {
			return argv[i+1];
		}
	}
	return NULL;
}

void print_hex(const void *data, int start, int count) 
{
	unsigned char *r = (unsigned char*)(data + start);

	for (int i = 0; i < count; i++)	{
 	    printf("%02X", r[i]);
	}
}


