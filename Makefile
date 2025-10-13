GENERATED_OUTPUT=Lox/Generated

.PHONY: all lox
all: astgen lox

astgen: ASTGen/*.cs
	dotnet run --project ASTGen $(GENERATED_OUTPUT)
	dotnet format Lox

lox: astgen
	dotnet run --project Lox