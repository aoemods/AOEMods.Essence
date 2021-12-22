using AOEMods.Essence.Chunky;
using AOEMods.Essence.Chunky.RRGeom;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AOEMods.Essence.Editor;

public class GeometryObjectViewModel : TabViewModel
{
    public Model3D? Content
    {
        get => content;
        set => SetProperty(ref content, value);
    }

    private Model3D? content = null;

    public GeometryObject? GeometryObject
    {
        get => geometryObject;
        set => SetProperty(ref geometryObject, value);
    }

    private GeometryObject? geometryObject = null;

    public ICommand ExportCommand { get; }

    public GeometryObjectViewModel()
    {
        ExportCommand = new RelayCommand(Export);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(GeometryObject))
        {
            if (GeometryObject != null)
            {
                var positions = new Point3DCollection(GeometryObject.VertexPositions.GetLength(0));
                var normals = new Vector3DCollection(GeometryObject.VertexNormals.GetLength(0));
                var texCoords = new PointCollection(GeometryObject.VertexTextureCoordinates.GetLength(0));
                var triangleIndices = new Int32Collection(GeometryObject.Faces.GetLength(0) * 3);

                for (int i = 0; i < GeometryObject.VertexPositions.GetLength(0); i++)
                {
                    positions.Add(new Point3D(
                        (double)GeometryObject.VertexPositions[i, 0],
                        (double)GeometryObject.VertexPositions[i, 1],
                        (double)GeometryObject.VertexPositions[i, 2]
                    ));
                }
                positions.Freeze();

                for (int i = 0; i < GeometryObject.VertexNormals.GetLength(0); i++)
                {
                    normals.Add(new Vector3D(
                        GeometryObject.VertexNormals[i, 0],
                        GeometryObject.VertexNormals[i, 1],
                        GeometryObject.VertexNormals[i, 2]
                    ));
                }
                normals.Freeze();

                for (int i = 0; i < GeometryObject.VertexTextureCoordinates.GetLength(0); i++)
                {
                    texCoords.Add(new Point(
                        (double)GeometryObject.VertexTextureCoordinates[i, 0],
                        (double)GeometryObject.VertexTextureCoordinates[i, 1]
                    ));
                }
                texCoords.Freeze();

                for (int i = 0; i < GeometryObject.Faces.GetLength(0); i++)
                {
                    triangleIndices.Add(GeometryObject.Faces[i, 0]);
                    triangleIndices.Add(GeometryObject.Faces[i, 1]);
                    triangleIndices.Add(GeometryObject.Faces[i, 2]);
                }
                triangleIndices.Freeze();

                var mesh = new MeshGeometry3D()
                {
                    Positions = positions,
                    Normals = normals,
                    TextureCoordinates = texCoords,
                    TriangleIndices = triangleIndices
                };

                var material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)));

                var model = new GeometryModel3D(mesh, material);

                Content = new Model3DGroup()
                {
                    Children = new Model3DCollection()
                    {
                        model,
                        new AmbientLight(Color.FromRgb(50, 50, 50)),
                        new DirectionalLight(Color.FromRgb(255, 255, 255), new Vector3D(3, -4, 5))
                    }
                };
            }
            else
            {
                Content = null;
            }
        }
    }

    private void Export()
    {
        if (GeometryObject != null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"obj (*.obj)|*.obj|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var fileStream = File.OpenWrite(saveFileDialog.FileName);
                ObjUtil.WriteGeometryObjectAsObj(fileStream, GeometryObject);
            }
        }
    }
}
