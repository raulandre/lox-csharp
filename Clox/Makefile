CC=gcc
CFLAGS=-Wall -fPIC -pedantic -ggdb
INCLUDE=-I./include/
LDFLAGS=
OUT=bin

TARGETS = chunk.o mem.o debug.o value.o vm.o compiler.o scanner.o

.PHONY: makebin clean run
all: makebin clox

run: all
	./$(OUT)/clox

clox: $(TARGETS) src/main.c
	$(CC) $(CFLAGS) $(INCLUDE) src/main.c bin/*.o $(LDFLAGS) -o $(OUT)/$@

chunk.o: src/chunk.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

mem.o: src/mem.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

debug.o: src/debug.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

value.o: src/value.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

vm.o: src/vm.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

compiler.o: src/compiler.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

scanner.o: src/scanner.c
	$(CC) -c $(CFLAGS) $(INCLUDE) $^ $(LDFLAGS) -o $(OUT)/$@

makebin:
	mkdir -p bin

clean:
	rm -rf bin/
