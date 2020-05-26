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
        private const int DIFFUSE_REFLECTION = 1;
        private const int MIRROR_REFLECTION = 2;
        private const int REFRACTION = 3;
        private const int countMaterials = 7;
        private Material[] materials = new Material[countMaterials];
        public int maxDepth = 10;

        public Material[] Materials {
            get {
                return materials;
            }
        }

        public void setMaterial(int i, Material material) {
            materials[i] = material;
        }

        private void fillMaterials() {
            Vector4 lightCoeffs = new Vector4(0.4f, 0.9f, 0.2f, 2.0f);
            materials[0] = new Material(new Vector3(0, 1, 0), lightCoeffs, 0.5f, 1, DIFFUSE_REFLECTION);
            materials[1] = new Material(new Vector3(1, 0, 0), lightCoeffs, 0.5f, 1, DIFFUSE_REFLECTION);
            materials[2] = new Material(new Vector3(0, 0, 1), lightCoeffs, 0.5f, 1, DIFFUSE_REFLECTION);
            materials[3] = new Material(new Vector3(1, 1, 0), lightCoeffs, 0.5f, 1, MIRROR_REFLECTION);
            materials[4] = new Material(new Vector3(0, 1, 1), lightCoeffs, 0.5f, 1, DIFFUSE_REFLECTION);
            materials[5] = new Material(new Vector3(1, 0, 1), lightCoeffs, 1, 1, MIRROR_REFLECTION);
            materials[6] = new Material(new Vector3(0.5f, 0.5f, 1), lightCoeffs, 1, 1.5f, REFRACTION);
        }

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
            initMaterials();
            initDepth();
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

        private void initMaterials() {
            for(int i = 0; i < countMaterials; i++) {
                int lm = GL.GetUniformLocation(BasicProgramID, "materials[" + i + "].Color");
                GL.Uniform3(lm, materials[i].Color);
                lm = GL.GetUniformLocation(BasicProgramID, "materials[" + i + "].LightCoeffs");
                GL.Uniform4(lm, materials[i].LightCoeffs);
                lm = GL.GetUniformLocation(BasicProgramID, "materials[" + i + "].ReflectionCoef");
                GL.Uniform1(lm, materials[i].ReflectionCoef);
                lm = GL.GetUniformLocation(BasicProgramID, "materials[" + i + "].RefractionCoef");
                GL.Uniform1(lm, materials[i].RefractionCoef);
                lm = GL.GetUniformLocation(BasicProgramID, "materials[" + i + "].MaterialType");
                GL.Uniform1(lm, materials[i].MaterilaType);
            }
        }

        private void initDepth() {
            int lm = GL.GetUniformLocation(BasicProgramID, "maxDepth");
            GL.Uniform1(lm, maxDepth);
        }

        public View() {
            fillMaterials();
        }
    }

    public struct Material {
        public Vector3 Color;
        public Vector4 LightCoeffs;
        public float ReflectionCoef;
        public float RefractionCoef;
        public int MaterilaType;
        public Material(Vector3 col, Vector4 Light, float reflection, float refraction, int type) {
            Color = col;
            LightCoeffs = Light;
            ReflectionCoef = reflection;
            RefractionCoef = refraction;
            MaterilaType = type;
        }
    }
}
