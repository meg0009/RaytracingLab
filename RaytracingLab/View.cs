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
        private const int countMaterials = 9;
        private const int countCubes = 2;
        private const int countSpheres = 2;
        private const int countTriangles = 12;
        private Material[] materials = new Material[countMaterials];
        private SSphere[] spheres = new SSphere[countSpheres];
        private STriangle[] triangles = new STriangle[countTriangles];
        private SCube[] cubes = new SCube[countCubes];
        public int maxDepth = 10;

        public Material[] Materials {
            get {
                return materials;
            }
        }

        public SSphere[] Spheres {
            get {
                return spheres;
            }
        }

        public STriangle[] Triangles {
            get {
                return triangles;
            }
        }

        public SCube[] Cubes {
            get {
                return cubes;
            }
        }

        public void setTriangleMaterial(int i, int material) {
            triangles[i].MaterialIdx = material;
        }

        public void setSphereMaterial(int i, int material) {
            spheres[i].MaterialIdx = material;
        }

        public void setCubeMaterial(int i, int material) {
            cubes[i].MaterialIdx = material;
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
            materials[7] = new Material(new Vector3(0.5f, 0.5f, 0.5f), lightCoeffs, 1, 1.5f, DIFFUSE_REFLECTION);
            materials[8] = new Material(new Vector3(0.5f, 0.5f, 1), lightCoeffs, 1, 1.5f, DIFFUSE_REFLECTION);
        }

        private void fillScene() {
            // Triangles
            // left wall
            triangles[0] = new STriangle(
                new Vector3(-5, -5, -8),
                new Vector3(-5, -5, 8),
                new Vector3(-5, 5, -8),
                0
            );
            triangles[1] = new STriangle(
                new Vector3(-5, -5, 8),
                new Vector3(-5, 5, 8),
                new Vector3(-5, 5, -8),
                0
            );

            // back wall
            triangles[2] = new STriangle(
                new Vector3(-5, -5, 8),
                new Vector3(5, -5, 8),
                new Vector3(-5, 5, 8),
                1
            );
            triangles[3] = new STriangle(
                new Vector3(5, -5, 8),
                new Vector3(5, 5, 8),
                new Vector3(-5, 5, 8),
                1
            );

            // right wall
            triangles[4] = new STriangle(
                new Vector3(5, -5, -8),
                new Vector3(5, -5, 8),
                new Vector3(5, 5, -8),
                2
            );
            triangles[5] = new STriangle(
                new Vector3(5, -5, 8),
                new Vector3(5, 5, 8),
                new Vector3(5, 5, -8),
                2
            );

            // bottom wall
            triangles[6] = new STriangle(
                new Vector3(-5, -5, -8),
                new Vector3(5, -5, -8),
                new Vector3(-5, -5, 8),
                3
            );
            triangles[7] = new STriangle(
                new Vector3(5, -5, -8),
                new Vector3(5, -5, 8),
                new Vector3(-5, -5, 8),
                3
            );

            // top wall
            triangles[8] = new STriangle(
                new Vector3(-5, 5, -8),
                new Vector3(5, 5, -8),
                new Vector3(-5, 5, 8),
                3
            );
            triangles[9] = new STriangle(
                new Vector3(5, 5, -8),
                new Vector3(5, 5, 8),
                new Vector3(-5, 5, 8),
                3
            );

            // front wall
            triangles[10] = new STriangle(
                new Vector3(-5, -5, -8),
                new Vector3(5, -5, -8),
                new Vector3(-5, 5, -8),
                1
            );
            triangles[11] = new STriangle(
                new Vector3(5, -5, -8),
                new Vector3(5, 5, -8),
                new Vector3(-5, 5, -8),
                1
            );

            // Spheres
            spheres[0] = new SSphere(
                new Vector3(-1, -1, -2),
                2,
                5
            );
            spheres[1] = new SSphere(
                new Vector3(2, 1, 2),
                1,
                6
            );

            // Cubes
            cubes[0] = new SCube(
                new Vector3(2, -4, -1),
                2,
                0,
                1,
                0,
                7
            );
            cubes[1] = new SCube(
                new Vector3(-1, -4.5f, -1.8f),
                1,
                0,
                0,
                1.5f,
                8
            );
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
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertdata.Length), vertdata, BufferUsageHint.DynamicDraw);
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
            initScene();
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

        private void initScene() {
            int lm;
            for(int i = 0; i < countTriangles; i++) {
                lm = GL.GetUniformLocation(BasicProgramID, "triangles[" + i + "].v1");
                GL.Uniform3(lm, triangles[i].v1);
                lm = GL.GetUniformLocation(BasicProgramID, "triangles[" + i + "].v2");
                GL.Uniform3(lm, triangles[i].v2);
                lm = GL.GetUniformLocation(BasicProgramID, "triangles[" + i + "].v3");
                GL.Uniform3(lm, triangles[i].v3);
                lm = GL.GetUniformLocation(BasicProgramID, "triangles[" + i + "].MaterialIdx");
                GL.Uniform1(lm, triangles[i].MaterialIdx);
            }
            for(int i = 0; i < countSpheres; i++) {
                lm = GL.GetUniformLocation(BasicProgramID, "spheres[" + i + "].Center");
                GL.Uniform3(lm, spheres[i].Center);
                lm = GL.GetUniformLocation(BasicProgramID, "spheres[" + i + "].Radius");
                GL.Uniform1(lm, spheres[i].Radius);
                lm = GL.GetUniformLocation(BasicProgramID, "spheres[" + i + "].MaterialIdx");
                GL.Uniform1(lm, spheres[i].MaterialIdx);
            }
            for(int i = 0; i < countCubes; i++) {
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].Center");
                GL.Uniform3(lm, cubes[i].Center);
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].Side");
                GL.Uniform1(lm, cubes[i].Side);
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].MaterialIdx");
                GL.Uniform1(lm, cubes[i].MaterialIdx);
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].ax");
                GL.Uniform1(lm, cubes[i].ax);
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].ay");
                GL.Uniform1(lm, cubes[i].ay);
                lm = GL.GetUniformLocation(BasicProgramID, "cubes[" + i + "].az");
                GL.Uniform1(lm, cubes[i].az);
            }
        }

        private void initDepth() {
            int lm = GL.GetUniformLocation(BasicProgramID, "maxDepth");
            GL.Uniform1(lm, maxDepth);
        }

        public View() {
            fillMaterials();
            fillScene();
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

    public struct STriangle {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
        public int MaterialIdx;
        public STriangle(Vector3 _v1, Vector3 _v2, Vector3 _v3, int material) {
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;
            MaterialIdx = material;
        }
    }

    public struct SSphere {
        public Vector3 Center;
        public float Radius;
        public int MaterialIdx;
        public SSphere(Vector3 center, float r, int material) {
            Center = center;
            Radius = r;
            MaterialIdx = material;
        }
    }

    public struct SCube {
        public Vector3 Center;
        public float Side;
        public float ax;
        public float ay;
        public float az;
        public int MaterialIdx;
        public SCube(Vector3 center, float side, float _ax, float _ay, float _az, int material) {
            Center = center;
            Side = side;
            ax = _ax;
            ay = _ay;
            az = _az;
            MaterialIdx = material;
        }
    }
}
