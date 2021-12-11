namespace AOEMods.Essence.Chunky.RRTex;

/// <summary>
/// Type of RRTex texture.
/// </summary>
public enum RRTexType
{
    /// <summary>
    /// Interpret image as is.
    /// </summary>
    Generic,

    /// <summary>
    /// Interpret AGB as RGB. Used in emi textures to encode normals.
    /// </summary>
    NormalMap,

    /// <summary>
    /// Interpret R as R. Used in emi textures to encode metallic.
    /// </summary>
    Metal,

    /// <summary>
    /// Interpret R as R. Used in pattern textures.
    /// </summary>
    Pattern,
};
