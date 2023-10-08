using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpmaid.Infrastructure;
using System.Text;
using Sharpmaid.Model;

namespace Sharpmaid.Service;

public class ClassService
{
    private Dictionary<string, ClassStructure> classes = new();

    public void ReadFiles(string path)
    {
        var fileExtension = ".cs";

        //todo read all files from all folders
        var files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            if (!File.Exists(file) || !file.EndsWith(fileExtension))
            {
                continue;
            }

            var code = IoHelper.ReadFile(file);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var classname = classDeclaration.Identifier.ToString();

                var props = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

                var methods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                var classStructure = new ClassStructure();
                classStructure.Name = classname;
                classStructure.ParentName = classDeclaration.BaseList?.Types.FirstOrDefault()?.ToString();
                classStructure.Properties = new List<(string text, string typename)>();
                classStructure.Methods = new List<string>();

                foreach (var csProp in props)
                {
                    var modifiers = GetModifiers(csProp.Modifiers);
                    var typename = csProp.Type.ToString();
                    var umlProperty = $"{modifiers.accessModifier}{csProp.Identifier.Text} {typename}{modifiers.additionalModifier}";

                    classStructure.Properties.Add((umlProperty, typename));
                }

                foreach (var csMethod in methods)
                {
                    var modifiers = GetModifiers(csMethod.Modifiers);

                    var parameters = string.Join(", ", csMethod.ParameterList.Parameters.Select(x => $"{x.Identifier.ValueText} {x.Type}"));

                    var umlMethod = $"{modifiers.accessModifier}{csMethod.Identifier.Text}({parameters}) {csMethod.ReturnType}{modifiers.additionalModifier}";

                    classStructure.Methods.Add(umlMethod);
                }

                classes.Add(classname, classStructure);
            }
        }
    }

    public void WriteUmlText(string path)
    {
        var openCurlyBrace = '{';
        var closeCurlyBrace = '}';

        List<string> result = new();
        List<string> classRelationships = new();

        result.Add("```mermaid");
        result.Add("classDiagram");
        foreach (var classStructure in classes)
        {
            if (!string.IsNullOrWhiteSpace(classStructure.Value.ParentName))
            {
                result.Add($"{classStructure.Value.ParentName}  <|-- {classStructure.Value.Name}");
            }

            if (classStructure.Value.Properties.Count == 0 && classStructure.Value.Methods.Count == 0)
            {
                continue;
            }

            result.Add($"class {classStructure.Key}{openCurlyBrace}");
            foreach (var property in classStructure.Value.Properties)
            {
                if (classes.ContainsKey(property.typename))
                {
                    classRelationships.Add($"{property.typename} o-- {classStructure.Key}");
                }
                result.Add(property.text);
            }

            foreach (var method in classStructure.Value.Methods)
            {
                result.Add(method);
            }

            result.Add($"{closeCurlyBrace}");
        }
        result.AddRange(classRelationships);

        result.Add("```");

        IoHelper.CreateFile(path, result);
    }

    private (char accessModifier, string additionalModifier) GetModifiers(SyntaxTokenList tokens)
    {
        bool isPublic = tokens.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        bool isPrivate = tokens.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
        bool isProtected = tokens.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));
        bool isInternal = tokens.Any(m => m.IsKind(SyntaxKind.InternalKeyword));

        bool isStatic = tokens.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        bool isAbstract = tokens.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        bool isVirtual = tokens.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));

        var accessModifier = isPublic ? '+' : isPrivate ? '-' : isProtected ? '#' : '~';

        var additionalModifierBuilder = new StringBuilder();
        if (isStatic)
        {
            additionalModifierBuilder.Append("$");
        }
        if (isAbstract)
        {
            additionalModifierBuilder.Append("*");
        }

        return (accessModifier, additionalModifierBuilder.ToString());
    }
}
