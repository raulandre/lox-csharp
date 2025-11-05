#ifndef COMPILER_H
#define COMPILER_H

#include <stdbool.h>
#include "chunk.h"

bool compile(const char *source, Chunk *chunk);

#endif // !COMPILER_H
