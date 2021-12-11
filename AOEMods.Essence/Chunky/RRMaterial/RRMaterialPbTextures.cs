namespace AOEMods.Essence.Chunky.RRMaterial;

/// <summary>
/// Physically based (PB) rendering textures of a material.
/// </summary>
/// <param name="Textures">Textures contained in the material. The keys are texture names and values are paths to the textures with in an SGA archive.</param>
public record RRMaterialPbTextures(IDictionary<string, string> Textures);