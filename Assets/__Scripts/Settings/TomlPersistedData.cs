using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Tomlyn;
using Tomlyn.Model;
using UnityEngine;

namespace SaturnGame.Settings
{
public abstract class TomlPersistedData<T> : SettingsWithTomlMetadata where T : TomlPersistedData<T>, new()
{
    [NotNull] protected abstract string TomlFile { get; }
    private string TomlPath => Path.Join(Application.persistentDataPath, TomlFile);

    protected abstract void AddTriviaToNewFile();
    
    [NotNull]
    public static T Load()
    {
        // Create a throwaway instance to get the path we should use. The path could be static but that doesn't work
        // with inheritance.
        string path = new T().TomlPath;

        T loadedToml = null;

        if (File.Exists(path))
        {
            try
            {
                FileInfo info = new(path);
                if (info.Length > 1_000_000)
                    // A normal settings file should be around 1KB, not 1MB.
                    // Something is seriously wrong, don't try to parse this.
                    throw new($"Not trying to load {path} - size is too large ({info.Length} bytes)");

                string tomlString = File.ReadAllText(path);
                loadedToml = Toml.ToModel<T>(tomlString, path);
            } catch (Exception e)
            {
                Debug.LogError($"Failed to load {typeof(T).Name} from {path}: {e}");

                // Move current settings file to a backup if possible, otherwise we will overwrite it.
                long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                File.Move(path, path + $"-{unixTime}.bak");
            }
        }

        if (loadedToml != null) return loadedToml;

        loadedToml = new();
        loadedToml.AddTriviaToNewFile();
        loadedToml.SaveToFile();

        return loadedToml;
    }

    public void SaveToFile()
    {
        // Note: Investigated rounding floats when writing:
        // - There is no way to change the internal serialization to string.
        // - TomlModelOptions.ConvertToToml can do arbitrary conversions during serialization, but at the end of the day
        //   the value still needs to be one of the supported primitives. Float and double will both have some
        //   imprecision you will see. String will be quoted. Decimal is not supported.
        // Concluded that there is no nice way to do this in TOML without using e.g. a string value.
        // Not worth the hassle.
        string tomlString = Toml.FromModel(this);
        File.WriteAllText(TomlPath, tomlString);
    }
}

public abstract class SettingsWithTomlMetadata : ITomlMetadataProvider {
    // TOML metadata information (comments, etc.) needed for ITomlMetadataProvider
    //TomlPropertiesMetadata ITomlMetadataProvider.PropertiesMetadata { get; set; }
    TomlPropertiesMetadata ITomlMetadataProvider.PropertiesMetadata { get; set; }

    public void SetLeadingTrivia([NotNull] string propertyName, List<TomlSyntaxTriviaMetadata> trivia)
    {
        ITomlMetadataProvider metadataProvider = this;
        metadataProvider.PropertiesMetadata ??= new();

        metadataProvider.PropertiesMetadata.TryGetProperty(propertyName, out TomlPropertyMetadata metadata);
        metadata ??= new();

        if (metadata.LeadingTrivia != null)
            metadata.LeadingTrivia.AddRange(trivia);
        else
            metadata.LeadingTrivia = trivia;

        metadataProvider.PropertiesMetadata.SetProperty(propertyName, metadata);
    }

    public void DebugTrivia([NotNull] string propertyName)
    {
        TomlPropertiesMetadata propertiesMetadata = ((ITomlMetadataProvider)this).PropertiesMetadata;
        if (propertiesMetadata == null)
        {
            Debug.Log($"{propertyName}: No properties metadata");
            return;
        }

        propertiesMetadata.TryGetProperty(propertyName, out TomlPropertyMetadata metadata);

        if (metadata == null)
        {
            Debug.Log($"{propertyName}: No metadata");
            return;
        }

        Debug.Log($"{propertyName}: {metadata.LeadingTrivia?.Count} leading trivia");
        foreach (TomlSyntaxTriviaMetadata trivia in metadata.LeadingTrivia ?? new List<TomlSyntaxTriviaMetadata>())
            Debug.Log(trivia);

        Debug.Log($"{propertyName}: {metadata.TrailingTriviaAfterEndOfLine?.Count} trailing trivia");
        foreach (TomlSyntaxTriviaMetadata trivia in metadata.TrailingTriviaAfterEndOfLine ?? new List<TomlSyntaxTriviaMetadata>())
            Debug.Log(trivia);

    }
}
}
