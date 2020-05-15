using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RaytracingLab {
    class View {
        private int BasicProgramID;
        private int BasicVertexShader;
        private int BasicFragmentShader;

        private void loadShader(String filename, ShaderType type, int program, out int address) {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename)) {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private void initShaders() {
            BasicProgramID = GL.CreateProgram();
            loadShader("..\\..\\raytracing.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("..\\..\\raytracing.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);
            GL.LinkProgram(BasicProgramID);
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
        }

        private Vector3[] vertdata = new Vector3[] {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(-1, 1, 0),
                new Vector3(1, 1, 0)
            };

        private int vboID;

        private void setupBuffers() {
            GL.GenBuffers(1, out vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertdata.Length), vertdata, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        }

        private int width;
        private int height;

        public void setupView(int width_, int height_) {
            width = width_;
            height = height_;
            GL.ShadeModel(ShadingModel.Smooth);
            //GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            initShaders();
            setupBuffers();
            GL.Viewport(0, 0, width, height);
        }

        public void Draw() {
            GL.ClearColor(System.Drawing.Color.AliceBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(BasicProgramID);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, vertdata.Length);
            GL.UseProgram(0);
        }

        public void Resize(int width_, int height_) {
            width = width_;
            height = height_;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.Viewport(0, 0, width, height);
        }
    }
}
