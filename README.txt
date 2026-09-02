Abra a pasta no Visual Studio (não no VSCode) para usar o Designer:
1. Abra Visual Studio (Community ou superior)
2. File -> Open -> Project/Solution -> selecione CrudApp.csproj ou a pasta
3. Restaure pacotes: Build -> Restore NuGet Packages
4. Build -> Build Solution
5. Run (F5) ou Publish para gerar .exe

Como gerar exe via linha de comando:
dotnet publish -c Release -r win-x64 --self-contained true

Observação:
- O projeto já referencia System.Data.SQLite.Core (versão 1.0.118). Se precisar de outra versão, ajuste o .csproj.
- O arquivo clientes.db será criado na pasta do executável ao rodar pela primeira vez.
