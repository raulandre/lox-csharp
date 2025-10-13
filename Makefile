GENERATED_OUTPUT=Lox/Generated

.PHONY: astgen lox
astgen:
	dotnet run --project ASTGen $(GENERATED_OUTPUT)
	dotnet format --include $(GENERATED_OUTPUT)

lox:
	dotnet run --project Lox