#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>

#include "fchunk.h"

int main( int argc, const char** argv)
{
	if (argc == 1) { 
		return 0; 
	}

	if (strcmp(argv[1], "read") == 0) {
		return read_chunk(argc, argv);	
	}

	if (strcmp(argv[1], "create") == 0) {
		return write_chunk(argc, argv);
		return 0;	
	}
}


int write_chunk(int argc, const char** argv) {
	unsigned int chunckLen= 0;
	int count=0;

	const char* nameIn = get_arg(argc, argv, "-in");
	if (nameIn == NULL) {
		printf("-in argument required");
		return -1;
	}
	const char* nameOut = get_arg(argc, argv, "-out");
	if (nameOut == NULL) {
		printf("-out argument required");
		return -1;
	}

	FILE *fpIn = fopen(nameIn, "r");
	if (fpIn == NULL) {
		printf("File %s not found", nameIn);
		return -1;
	}

	FILE *fpOut = stdout;
	fpOut = fopen(nameOut, "a+");

	fseek(fpIn, 0L, SEEK_END);
	unsigned int chunkLen = ftell(fpIn);

	int bytes = 4;
	while(bytes-- > 0 && feof(fpIn)==0) {
		unsigned char byte = (unsigned char)(chunkLen&0xFF);
		if (fpOut != NULL) {
			fputc(byte, fpOut);
		}
		chunkLen = chunkLen >> 8;
	}

	fseek(fpIn, 0L, SEEK_SET);
	unsigned char buf[32768];
	while (feof(fpIn)==0) {
		unsigned int nread = fread(buf, 1, 32768, fpIn);
		if (nread == 0) 
			break;

		unsigned char *pwrite = buf;
		unsigned int nwrite=nread;		
		while(nwrite != 0) {		
			unsigned int nwritten = fwrite(pwrite, 1, nwrite, fpOut);
			nwrite -= nwritten;
			pwrite += nwritten;
		}
	}

	fclose(fpIn);
	fclose(fpOut);
	return 0;
}

int read_chunk(int argc, const char** argv) {

	const char* nameIn = get_arg(argc, argv, "-in");
	if (nameIn == NULL) {
		printf("-in argument required");
		return -1;
	}
	const char* nameOut = get_arg(argc, argv, "-out");
	if (nameOut == NULL) {
		printf("-out argument required");
		return -1;
	}

	unsigned int chunkLen= 0;

	FILE *fpOut = fopen(nameOut, "w");
	FILE *fpIn = fopen(nameIn, "r");
	const char* strSeek = get_arg(argc, argv, "-seek");
	if (strSeek != NULL) {
		long seek = strtol(strSeek, NULL, 10);
		fseek(fpIn, seek, SEEK_SET);
	}

	int bytes=4;
	while(bytes-- > 0 && feof(fpIn)==0) {
		int ch=getc(fpIn);
		if (ch == EOF)
			break;

		chunkLen = (chunkLen >> 8) | (ch << 24);
	}

	

	while(chunkLen > 0 && feof(fpIn)==0) {
		int ch=getc(fpIn);
		if (ch == EOF)
			break;

		fputc(ch, fpOut);
		chunkLen--;
	}
	
	printf("%ld", ftell(fpIn));	

	if (fpOut != stdout) {
		fclose(fpOut);
	}
	if (fpIn != stdin) {
		fclose(fpIn);
	}
	return 0;
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


