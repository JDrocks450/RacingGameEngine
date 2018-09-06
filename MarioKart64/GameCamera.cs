using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartEngine
{
    public class GameCamera : GameComponent
    {
        public Vector3 Position = new Vector3(0, 40, 20);
        public Vector3 LookAt = Vector3.Zero;
        public Vector3 Up = Vector3.UnitZ;
        public BasicEffect PerspectiveShader;

        float leftrightRot = MathHelper.PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        const float moveSpeed = 5.0f;

        float aspectRatio;
        float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
        float nearClipPlane = 1;
        float farClipPlane = 10000;

        GraphicsDeviceManager _graphics;
        private Point _lastMousePos;

        public BasicEffect GetEffect
        {
            get
            {
                if (PerspectiveShader is null)
                    PerspectiveShader = new BasicEffect(_graphics.GraphicsDevice);
                PerspectiveShader.View = GetView;
                PerspectiveShader.Projection = GetProjection;                
                return PerspectiveShader;
            }
        }

        public Matrix GetView
        {
            get => Matrix.CreateLookAt(Position, LookAt, Up);
        }

        public Matrix GetProjection
        {
            get => Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }
        public float CameraSpeed { get; private set; } = 1f;
        public bool IsLockingMouse { get; set; } = false;        

        public GameCamera(GraphicsDeviceManager graphics)
        {
            aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            _graphics = graphics;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            FREECAMERA_ControlCamera(delta);
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationZ(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0,-1,0);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            LookAt = Position + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = Vector3.UnitZ;
            Up = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
        }

        public void MoveCameraWithLookAt(Vector3 Offset)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationZ(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(Offset, cameraRotation);
            Position += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        void FREECAMERA_ControlCamera(float delta)
        {
            var currentMouseState = Mouse.GetState();
            var change = new Vector2(currentMouseState.X - _lastMousePos.X, currentMouseState.Y - _lastMousePos.Y);
            if (IsLockingMouse)
            {
                Mouse.SetPosition(Main.CenterScreen.X, Main.CenterScreen.Y);
                _lastMousePos = new Point(Main.CenterScreen.X, Main.CenterScreen.Y);
            }
            else
                _lastMousePos = currentMouseState.Position;
            if (change != Vector2.Zero)
            {
                leftrightRot -= CameraSpeed * change.X * delta;
                updownRot += CameraSpeed * change.Y * delta;
                UpdateViewMatrix();
            }            
            var mVector = Vector3.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    mVector.Z++;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    mVector.Z--;
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    mVector.Y--;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    mVector.Y++;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                mVector.X++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                mVector.X--;
            }
            MoveCameraWithLookAt(mVector);
        }
    }
}
