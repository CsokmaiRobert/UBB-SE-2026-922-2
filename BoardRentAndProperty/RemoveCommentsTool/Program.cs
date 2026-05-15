using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

static class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: RemoveCommentsTool <path-to-root>");
            return 1;
        }

        var rootPath = args[0];
        var csFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            try
            {
                var text = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(text);
                var root = tree.GetRoot();

                var rewriter = new CommentRemovingRewriter();
                var newRoot = rewriter.Visit(root);

                var newText = newRoot.ToFullString();
                File.WriteAllText(file, newText);
                Console.WriteLine($"Processed: {file}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {file} - {ex.Message}");
            }
        }

        return 0;
    }
}

class CommentRemovingRewriter : CSharpSyntaxRewriter
{
    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
        {
            return default(SyntaxTrivia);
        }

        return trivia;
    }
}
