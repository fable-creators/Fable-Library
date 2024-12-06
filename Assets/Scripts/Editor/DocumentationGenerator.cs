using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Editor utility for generating documentation from C# scripts in Unity projects.
/// Specifically tailored for MediaBuilder extension documentation.
/// </summary>
public class DocumentationGenerator : EditorWindow
{
    private string outputPath = "Documentation";
    private bool includePrivateMembers = false;
    private bool generateMarkdown = true;
    private bool includeInheritedMembers = true;
    private Vector2 scrollPosition;
    private List<MonoScript> scriptsToDocument = new List<MonoScript>();

    [MenuItem("Window/MediaBuilder/Documentation Generator", false, 2000)]
    public static void ShowWindow()
    {
        GetWindow<DocumentationGenerator>("Documentation Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("MediaBuilder Documentation Generator", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        includePrivateMembers = EditorGUILayout.Toggle("Include Private Members", includePrivateMembers);
        generateMarkdown = EditorGUILayout.Toggle("Generate Markdown", generateMarkdown);
        includeInheritedMembers = EditorGUILayout.Toggle("Include Inherited Members", includeInheritedMembers);

        EditorGUILayout.Space();
        if (GUILayout.Button("Add All Project Scripts"))
        {
            AddAllProjectScripts();
        }

        EditorGUILayout.Space();
        DisplayScriptList();

        if (GUILayout.Button("Generate Documentation"))
        {
            GenerateDocumentation();
        }
    }

    private void AddAllProjectScripts()
    {
        scriptsToDocument.Clear();
        string targetFolder = "Assets/Scripts";
        string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { targetFolder });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() != null)
            {
                scriptsToDocument.Add(script);
            }
        }
    }

    private void DisplayScriptList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = scriptsToDocument.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();
            scriptsToDocument[i] = (MonoScript)EditorGUILayout.ObjectField(
                scriptsToDocument[i], typeof(MonoScript), false);
                
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                scriptsToDocument.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void GenerateDocumentation()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        foreach (MonoScript script in scriptsToDocument)
        {
            if (script == null) continue;
            
            Type scriptType = script.GetClass();
            if (scriptType == null) continue;

            string documentation = generateMarkdown ? 
                GenerateMarkdownDoc(scriptType) : 
                GenerateTextDoc(scriptType);

            string fileName = $"{scriptType.Name}{(generateMarkdown ? ".md" : ".txt")}";
            string fullPath = Path.Combine(outputPath, fileName);
            
            File.WriteAllText(fullPath, documentation);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Documentation generated at {Path.GetFullPath(outputPath)}");
    }

    private string GenerateMarkdownDoc(Type type)
    {
        StringBuilder sb = new StringBuilder();

        // Class documentation
        sb.AppendLine($"# {type.Name}");
        sb.AppendLine();
        
        var classAttributes = type.GetCustomAttributes(typeof(TooltipAttribute), false);
        if (classAttributes.Length > 0)
        {
            sb.AppendLine(((TooltipAttribute)classAttributes[0]).tooltip);
            sb.AppendLine();
        }

        // Inheritance
        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            sb.AppendLine($"Inherits from: `{type.BaseType.Name}`");
            sb.AppendLine();
        }

        // Properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | 
            BindingFlags.Instance | BindingFlags.Static);
        if (properties.Length > 0)
        {
            sb.AppendLine("## Properties");
            sb.AppendLine();
            foreach (var property in properties)
            {
                if (!includePrivateMembers && !(property.GetMethod?.IsPublic ?? property.SetMethod?.IsPublic ?? false)) continue;
                if (!includeInheritedMembers && property.DeclaringType != type) continue;

                sb.AppendLine($"### {property.Name}");
                sb.AppendLine($"- Type: `{property.PropertyType.Name}`");
                sb.AppendLine($"- Access: {GetAccessModifier(property)}");
                
                var attributes = property.GetCustomAttributes(typeof(TooltipAttribute), false);
                if (attributes.Length > 0)
                {
                    sb.AppendLine($"- Description: {((TooltipAttribute)attributes[0]).tooltip}");
                }
                sb.AppendLine();
            }
        }

        // Methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
            BindingFlags.Instance | BindingFlags.Static);
        if (methods.Length > 0)
        {
            sb.AppendLine("## Methods");
            sb.AppendLine();
            foreach (var method in methods)
            {
                if (!includePrivateMembers && !method.IsPublic) continue;
                if (!includeInheritedMembers && method.DeclaringType != type) continue;
                if (method.IsSpecialName) continue; // Skip property accessors

                sb.AppendLine($"### {method.Name}");
                sb.AppendLine($"```csharp");
                sb.AppendLine(GenerateMethodSignature(method));
                sb.AppendLine($"```");
                
                var attributes = method.GetCustomAttributes(typeof(TooltipAttribute), false);
                if (attributes.Length > 0)
                {
                    sb.AppendLine($"Description: {((TooltipAttribute)attributes[0]).tooltip}");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string GenerateTextDoc(Type type)
    {
        // Similar to markdown but without formatting
        StringBuilder sb = new StringBuilder();
        // ... Implementation similar to markdown but without special formatting
        return sb.ToString();
    }

    private string GenerateMethodSignature(MethodInfo method)
    {
        StringBuilder sb = new StringBuilder();
        
        // Access modifier
        if (method.IsPublic) sb.Append("public ");
        else if (method.IsPrivate) sb.Append("private ");
        else if (method.IsFamily) sb.Append("protected ");
        
        // Static modifier
        if (method.IsStatic) sb.Append("static ");
        
        // Return type
        sb.Append(method.ReturnType.Name + " ");
        
        // Method name and parameters
        sb.Append(method.Name + "(");
        
        ParameterInfo[] parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (i > 0) sb.Append(", ");
            sb.Append(param.ParameterType.Name + " " + param.Name);
        }
        
        sb.Append(")");
        return sb.ToString();
    }

    private string GetAccessModifier(PropertyInfo property)
    {
        if (property.GetMethod?.IsPublic ?? false) return "Public";
        if (property.GetMethod?.IsPrivate ?? false) return "Private";
        if (property.GetMethod?.IsFamily ?? false) return "Protected";
        return "Unknown";
    }
}