GENERATED_OUTPUT=Lox/Generated

.PHONY: lox
astgen: ASTGen/**.cs
	dotnet run --project ASTGen $(GENERATED_OUTPUT)
	dotnet format Lox

lox:
	dotnet run --project Lox