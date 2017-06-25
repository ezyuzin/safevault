CFLAGS=-std=c99
CFLAG +=-Wall -pedantic
LIBS +=-lm

CC=gcc
CC=/usr/local/arm-none-linux-gnueabi/bin/arm-none-linux-gnueabi-gcc
LD=/usr/local/arm-none-linux-gnueabi/bin/arm-none-linux-gnueabi-ld
CFLAGS += -I/usr/local/arm-none-linux-gnueabi/arm-none-linux-gnueabi/libc/include
LDFLAGS += -L/usr/local/arm-none-linux-gnueabi/arm-none-linux-gnueabi/libc/lib

SOURCES += fchunk.c 
#SOURCES += crypto/*.c

OBJECTS=$(SOURCES:.c=.o)
EXEC=fchunk

all:
	$(CC) $(CFLAGS) $(SOURCES) $(LIBS) -o $(EXEC)

run: all
	./$(EXEC) 

.PHONY : clean all
