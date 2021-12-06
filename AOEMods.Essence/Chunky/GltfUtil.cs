using AOEMods.Essence.Chunky.RRGeom;
using AOEMods.Essence.Chunky.RRTex;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using System.Numerics;

namespace AOEMods.Essence.Chunky;

public static class GltfUtil
{
    public static MaterialBuilder MaterialFromTextures(TextureMip? diffuseTexture, TextureMip? normalTexture, TextureMip? metalTexture)
    {
        if (diffuseTexture != null)
        {
            var material = new MaterialBuilder()
                .WithMetallicRoughnessShader()
                .WithChannelImage(KnownChannel.BaseColor, diffuseTexture.Value.Data);

            if (normalTexture != null)
            {
                material = material.WithNormal(normalTexture.Value.Data);
            }

            if (metalTexture != null)
            {
                material = material.WithMetallicRoughness(metalTexture.Value.Data);
            }

            return material;
        }

        return MaterialBuilder.CreateDefault();
    }

    public static ModelRoot GeometryObjectToModel(GeometryObject geometryObject, MaterialBuilder material)
    {
        var meshBuilder = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>.CreateCompatibleMesh();
        var primitive = meshBuilder.UsePrimitive(material);

        var verts = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>[geometryObject.VertexPositions.GetLength(0)];
        for (int i = 0; i < geometryObject.VertexPositions.GetLength(0); i++)
        {
            verts[i] = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>
                .Create(
                    new Vector3(
                        (float)geometryObject.VertexPositions[i, 0],
                        (float)geometryObject.VertexPositions[i, 1],
                        (float)geometryObject.VertexPositions[i, 2]
                    ),
                    new Vector3(
                        geometryObject.VertexNormals[i, 0],
                        geometryObject.VertexNormals[i, 1],
                        geometryObject.VertexNormals[i, 2]
                    )
                ).WithMaterial(new Vector2(
                    (float)geometryObject.VertexTextureCoordinates[i, 0],
                    (float)geometryObject.VertexTextureCoordinates[i, 1]
                ));
        }

        for (int i = 0; i < geometryObject.Faces.GetLength(0); i++)
        {
            primitive.AddTriangle(
                verts[geometryObject.Faces[i, 0]],
                verts[geometryObject.Faces[i, 1]],
                verts[geometryObject.Faces[i, 2]]
            );
        }
        primitive.Validate();

        var model = ModelRoot.CreateModel();
        var mesh = model.CreateMeshes(meshBuilder).Single();
        var scene = model.UseScene(0);
        scene.CreateNode().WithMesh(mesh);

        return model;
    }
}
