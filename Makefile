GENERATED_OUTPUT=Lox/Generated

astgen: ASTGen/**.cs
	dotnet run --project ASTGen $(GENERATED_OUTPUT)
	dotnet format --include $(GENERATED_OUTPUT)

lox: Lox/**.cs	
	dotnet run --project Lox