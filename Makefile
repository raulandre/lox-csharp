GENERATED_OUTPUT=Lox/Generated
TEST_SCRIPT=Script.lox

.PHONY: all lox
all: astgen lox

astgen: ASTGen/*.cs
	dotnet run --project ASTGen $(GENERATED_OUTPUT)
	dotnet format Lox

lox: 
	dotnet run --configuration Release --project Lox $(CURDIR)/$(TEST_SCRIPT)