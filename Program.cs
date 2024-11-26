using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if (args.Length != 2)
{
    Console.WriteLine("Invalid number of arguments");
    return;
}

string folderPath1 = args[0];
string folderPath2 = args[1];

if (!Directory.Exists(folderPath1))
{
    Console.WriteLine("Directory not found: " + folderPath1);
    return;
}

if (!Directory.Exists(folderPath2))
{
    Console.WriteLine("Directory not found: " + folderPath2);
    return;
}

int count = 0;
var files = Directory.EnumerateFiles(folderPath1, "*.cs", SearchOption.AllDirectories);

foreach (var file1 in files)
{
    if (file1.Contains("/bin/") || file1.Contains("/obj/"))
        continue;

    count++;

    if (count % 1000 == 0)
        Console.WriteLine($"Processed {count} files");

    var relativePath = Path.GetRelativePath(folderPath1, file1);
    var file2 = Path.Combine(folderPath2, relativePath);

    if (File.Exists(file2))
    {
        var node1 = await GetNormalizedRootAsync(file1);
        var node2 = await GetNormalizedRootAsync(file2);

        bool textEquals = StringComparer.Ordinal.Equals(node1.ToFullString(), node2.ToFullString());

        if (!textEquals)
            Console.WriteLine(relativePath);
    }
    else
    {
        Console.WriteLine($"File not found: {file2}");
    }
}

static async Task<SyntaxNode> GetNormalizedRootAsync(string path)
{
    var code = await File.ReadAllTextAsync(path);
    var tree = CSharpSyntaxTree.ParseText(code);
    var root = await tree.GetRootAsync();
    var normalizedRoot = root.NormalizeWhitespace(indentation: "", eol: " ", elasticTrivia: false);

    return normalizedRoot;
}