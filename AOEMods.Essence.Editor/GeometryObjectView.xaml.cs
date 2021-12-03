using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AOEMods.Essence.Editor
{
    /// <summary>
    /// Interaction logic for GeometryObjectView.xaml
    /// </summary>
    public partial class GeometryObjectView : UserControl
    {
        public GeometryObjectViewModel ViewModel => (GeometryObjectViewModel)DataContext;

        public GeometryObjectView()
        {
            InitializeComponent();
        }

        private const float CameraMoveSpeed = 0.1f;
        private const float CameraRotateSpeed = 0.003f;
        bool rotating = false;
        Point previousMousePosition;

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (rotating)
            {
                var newMousePosition = e.GetPosition(null);
                var deltaPosition = newMousePosition - previousMousePosition;

                var oldForward = new Vector3(
                    (float)camera.LookDirection.X,
                    (float)camera.LookDirection.Y,
                    (float)camera.LookDirection.Z
                );

                // Rotate mouse horizontal (yaw)
                // Rotate mouse vertical (pitch)
                var rotor = Quaternion.CreateFromYawPitchRoll(
                    -CameraRotateSpeed * (float)deltaPosition.X,
                    CameraRotateSpeed * (float)deltaPosition.Y,
                    0
                );

                var newForward = Vector3.Transform(oldForward, rotor);

                // Apply new vectors
                camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(
                    newForward.X, newForward.Y, newForward.Z
                );

                previousMousePosition = newMousePosition;

                e.Handled = true;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            rotating = true;
            previousMousePosition = e.GetPosition(null);
            e.MouseDevice.Capture(this);
            e.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rotating)
            {
                rotating = false;
                e.MouseDevice.Capture(null);
                e.Handled = true;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (rotating)
            {
                if (e.Key == Key.W)
                {
                    camera.Position += CameraMoveSpeed * camera.LookDirection;
                    e.Handled = true;
                }

                if (e.Key == Key.S)
                {
                    camera.Position -= CameraMoveSpeed * camera.LookDirection;
                    e.Handled = true;
                }

                if (e.Key == Key.A)
                {
                    camera.Position -= CameraMoveSpeed * System.Windows.Media.Media3D.Vector3D.CrossProduct(
                        camera.LookDirection, camera.UpDirection
                    );
                    e.Handled = true;
                }

                if (e.Key == Key.D)
                {
                    camera.Position += CameraMoveSpeed * System.Windows.Media.Media3D.Vector3D.CrossProduct(
                        camera.LookDirection, camera.UpDirection
                    );
                    e.Handled = true;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += OnKeyDown;
        }
    }
}
