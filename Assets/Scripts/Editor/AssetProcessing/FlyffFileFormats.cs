using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uFlyff.Editor.AssetProcessing
{
    /// <summary>
    /// Represents one of Flyff's proprietary file formats and it's Object 
    /// equivalent, as well as how to load and save this file structure.
    /// </summary>
    /// <typeparam name="T">The type of Unity object this format will be converted to</typeparam>
    internal abstract class FlyffFileFormat<T> where T : UnityEngine.Object
    {
        protected string assetPath;
        protected bool wasLoaded = false;

        public FlyffFileFormat(string filePath)
        {
            this.assetPath = filePath;
            Load(filePath);
            
        }

        public abstract void Load(string filePath);
        public abstract void Save(string filePath);

        public abstract T GetAssetObject();

        public Type GetAssetObjectType()
        {
            return typeof(T);
        }

        public string GetAssetPath()
        {
            return this.assetPath;
        }
    }
     

    internal class o3d : FlyffFileFormat<Mesh>
    {
        //-------------------------------------------------------------------------------//
        // Mesh File Structure                                                           //
        //-------------------------------------------------------------------------------//
        public byte nameLength;
        public char[] encryptedName; // = new char[nameLength];

        public int version;
        public int unknown1;
        public Vector3 element1Start;
        public Vector3 element1End;

        // If version == 22
        public Vector3 element2Start;
        public Vector3 element2End;

        public int unk2;
        public int unk3;
        public byte[] fileName;//new char[16];

        public Vector3 bbMin;
        public Vector3 bbMax;

        public float unk4; //always 0.5
        public int flag1;
        public int flag2;
        public Vector3[] flag2Data;

        public int MeshTypeFlag;
        public int LODFlag;
        public int unk5;
        public int TotalMeshCount; // all mesh and LOD
        public int MeshCount;

        public LODMesh[] lods;

        public class LODMesh
        {
            public int hasBones; // if this is 0x01, model is animated
            public int nCount3;

            public int[] nData3;//= new int[nCount3]
            public int unknown1;
            public int nTerminator;

            public Matrix4x4 transform;

            public Vector3 bbMin;
            public Vector3 bbMax;

            public int unk2;
            public bool opacity;
            public bool bump;
            public bool rigid;
            //public byte[] unknown2; // = new byte[40]

            public int vertexListSize;
            public int vertexCount;
            public int faceListSize;
            public int indexCount;

            public Vector3[] vertexList; // = new Vector3[XCount];
            public class VertPool
            {
                public Vector3 position;
                public float weight1;
                public float weight2;
                public ushort bone1;
                public ushort bone2;
                public Vector3 normal;
                public Vector2 UV;
            }

            public VertPool[] verticies; // = new VertPool[VtCount]

            public ushort[] faces; // = new Face[FcCount]
            public int hasPhysique;
            public ushort[] triangles; // = new int[VtCount];

            public int[] physiqueVerticies; // = new int[XCount]

            //----materials----//
            public int hasMaterials;
            public int materialCount;
            public class Material
            {
                //{
                public Vector4 diffuse;
                public Vector4 ambient;
                public Vector4 specular;
                public Vector4 emissive;
                public float materialPower;
                //} is a D3DMATERIAL9
                public int textureNameLen;
                public byte[] textureName;
            }
            public Material[] materials;// = new Material[MatertialCount];

            public int matIdCount;
            public class FaceMatId
            {
                public int startingVertex;
                public int primitiveCount;
                public int materialId;
                public uint effect;
                public int amount;
                public int usedBoneCount;
                public int[] usedBones;// = new int[28];
            }
            public FaceMatId[] faceMats; // = new FaceMatId[matIdCount];

        }

        public int matIdCount;
        public class FaceMatId
        {
            public int startingVertex;
            public int primitiveCount;
            public int materialId;
            public uint effect;
            public int amount;
            public int usedBoneCount;
            public int[] usedBones;// = new int[28];
        }
        public FaceMatId[] faceMats; // = new FaceMatId[matIdCount];
        //-------------------------------------------------------------------------------//

        public o3d(string filePath) : base(filePath)
        {}

        public override Mesh GetAssetObject()
        {
            Mesh mesh = new Mesh();
            if (this.wasLoaded)
            {
                LODMesh lod = lods[0];
                List<Vector3> verts = new List<Vector3>();
                List<Vector3> norms = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<BoneWeight> weights = new List<BoneWeight>();

                for(int i = 0; i < lod.verticies.Length; i++)
                {
                    verts.Add(lod.verticies[i].position);
                    uvs.Add(lod.verticies[i].UV);
                    norms.Add(lod.verticies[i].normal);

                    if (lod.hasBones == 1)
                    {
                        BoneWeight weight = new BoneWeight();
                        weight.boneIndex0 = lod.verticies[i].bone1;
                        weight.boneIndex1 = lod.verticies[i].bone2;
                        weight.weight0 = lod.verticies[i].weight1;
                        weight.weight1 = lod.verticies[i].weight2;
                        weights.Add(weight);
                    }
                }

                mesh.SetVertices(verts);
                mesh.SetNormals(norms);
                mesh.uv = uvs.ToArray();

                if (lod.hasBones == 1)
                    mesh.boneWeights = weights.ToArray();

                int numTris = lod.indexCount;
                int[] triangles = new int[numTris];
                for(int j = 0; j < numTris; j++)
                {
                    triangles[j] = (int)lod.faces[j];
                }

                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
            }
            return mesh;
        }

        public override void Load(string filePath)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            nameLength = reader.ReadByte();
            fileName = reader.ReadBytes(nameLength);

            version = reader.ReadInt32();
            unknown1 = reader.ReadInt32();

            element1Start = reader.ReadVector3();
            element1End = reader.ReadVector3();

            if (version == 0x16)
            {
                element2Start = reader.ReadVector3();
                element2End = reader.ReadVector3();
            }

            unk2 = reader.ReadInt32();
            unk3 = reader.ReadInt32();
            reader.Skip(16);

            bbMin = reader.ReadVector3();
            bbMax = reader.ReadVector3();

            unk4 = reader.ReadSingle();

            flag1 = reader.ReadInt32();// nAnimation States
            flag2 = reader.ReadInt32(); // animationEventCount?
            flag2Data = new Vector3[flag2];
            for (int i = 0; i < flag2; i++)
                flag2Data[i] = reader.ReadVector3();

            MeshTypeFlag = reader.ReadInt32();
            LODFlag = reader.ReadInt32();
            unk5 = reader.ReadInt32();
            TotalMeshCount = reader.ReadInt32();

            MeshCount = reader.ReadInt32();

            lods = new LODMesh[MeshCount];
            for (int i = 0; i < MeshCount; i++)
            {
                LODMesh lod = lods[i] = new LODMesh();
                lod.hasBones = reader.ReadInt32();

                lod.nCount3 = reader.ReadInt32();
                lod.nData3 = new int[lod.nCount3];
                for (int j = 0; j < lod.nCount3; j++)
                    lod.nData3[j] = reader.ReadInt32();
                lod.unknown1 = reader.ReadInt32();
                lod.nTerminator = reader.ReadInt32();
                lod.transform = reader.ReadMatrix4x4();
                lod.bbMin = reader.ReadVector3();
                lod.bbMax = reader.ReadVector3();

                lod.unk2 = reader.ReadInt32();
                lod.opacity = lod.unk2 != 0;

                lod.unk2 = reader.ReadInt32();
                lod.bump = lod.unk2 != 0;

                lod.unk2 = reader.ReadInt32();
                lod.rigid = lod.unk2 != 0;

                reader.Skip(28);

                lod.vertexListSize = reader.ReadInt32();
                lod.vertexCount = reader.ReadInt32();
                lod.faceListSize = reader.ReadInt32();
                lod.indexCount = reader.ReadInt32();

                // xData is the verticies
                lod.vertexList = new Vector3[lod.vertexListSize];
                for (int j = 0; j < lod.vertexListSize; j++)
                    lod.vertexList[j] = reader.ReadVector3();

                // these are the verticies for the graphics buffer
                lod.verticies = new LODMesh.VertPool[lod.vertexCount];
                for (int j = 0; j < lod.vertexCount; j++)
                {
                    LODMesh.VertPool vert = lod.verticies[j] = new LODMesh.VertPool();
                    vert.position = reader.ReadVector3();
                    if (lod.hasBones == 1)
                    {
                        vert.weight1 = reader.ReadSingle();
                        vert.weight2 = reader.ReadSingle();
                        vert.bone1 = reader.ReadUInt16();
                        vert.bone2 = reader.ReadUInt16();
                    }
                    vert.normal = reader.ReadVector3();
                    vert.UV = reader.ReadVector2();
                }

                // faces = indicies
                lod.faces = new ushort[lod.indexCount];
                for (int j = 0; j < lod.indexCount; j++)
                    lod.faces[j] = reader.ReadUInt16();

                lod.triangles = new ushort[lod.vertexCount];
                for (int j = 0; j < lod.vertexCount; j++)
                    lod.triangles[j] = reader.ReadUInt16();

                lod.hasPhysique = reader.ReadInt32();
                if (lod.hasPhysique != 0)
                {
                    lod.physiqueVerticies = new int[lod.vertexListSize];
                    for (int j = 0; j < lod.vertexListSize; j++)
                        lod.physiqueVerticies[j] = reader.ReadInt32();
                }

                // materials

                lod.hasMaterials = reader.ReadInt32();
                if (lod.hasMaterials != 0)
                {
                    lod.materialCount = reader.ReadInt32();
                    if (lod.materialCount == 0)
                        lod.materialCount = 1;

                    lod.materials = new LODMesh.Material[lod.materialCount];
                    for (int j = 0; j < lod.materialCount; j++)
                    {
                        LODMesh.Material mat = lod.materials[j] = new LODMesh.Material();
                        mat.diffuse = reader.ReadVector4();
                        mat.ambient = reader.ReadVector4();
                        mat.specular = reader.ReadVector4();
                        mat.emissive = reader.ReadVector4();
                        mat.materialPower = reader.ReadSingle();
                        mat.textureNameLen = reader.ReadInt32();
                        mat.textureName = reader.ReadBytes(mat.textureNameLen);
                    }
                }

                lod.matIdCount = reader.ReadInt32();
                if (lod.matIdCount > 0)
                {
                    lod.faceMats = new LODMesh.FaceMatId[lod.matIdCount];
                    for (int j = 0; j < lod.matIdCount; j++)
                    {
                        LODMesh.FaceMatId faceMat = lod.faceMats[j] = new LODMesh.FaceMatId();
                        faceMat.startingVertex = reader.ReadInt32();
                        faceMat.primitiveCount = reader.ReadInt32();
                        faceMat.materialId = reader.ReadInt32();
                        faceMat.effect = reader.ReadUInt32();
                        faceMat.amount = reader.ReadInt32();
                        faceMat.usedBoneCount = reader.ReadInt32();
                        faceMat.usedBones = new int[faceMat.usedBoneCount];
                        for (int k = 0; k < faceMat.usedBoneCount; k++)
                            faceMat.usedBones[k] = reader.ReadInt32();
                    }

                }
            }

            reader.Close();
            this.wasLoaded = true;
        }

        public override void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }

    internal class ani : FlyffFileFormat<AnimationClip>
    {
        //-------------------------------------------------------------------------------//
        // Anim File Structure                                                           //
        //-------------------------------------------------------------------------------//
        public int version;
        public int id;
        public float perSlerp;
        public int boneCount;
        public int frameCount;

        public Vector3[] paths;

        public class MotionAttribute
        {
            public uint type;
            public int soundId;
            public float frame;
        }

        public MotionAttribute[] attributes;

        public Vector3[] events;
        int eventCount;

        public class Bone
        {
            public int parentId;
            public string name;
            public Matrix4x4 localTransform;
            public Matrix4x4 transform;
            public Matrix4x4 inverseTransform;
        }

        public Bone[] bones; // new Bone[boneCount]

        public class TMAnimation
        {
            public Quaternion rotation;
            public Vector3 position;
        }

        public TMAnimation[] animations;

        public class BoneFrame
        {
            public TMAnimation[] frames;
            public Matrix4x4 transform;
        }

        BoneFrame[] frames;
        //-------------------------------------------------------------------------------//


        public ani(string filePath) : base(filePath)
        {}

        public override AnimationClip GetAssetObject()
        {
            throw new NotImplementedException();
        }

        public override void Load(string filePath)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            version = reader.ReadInt32();
            id = reader.ReadInt32();
            perSlerp = reader.ReadSingle();

            reader.Skip(32);

            boneCount = reader.ReadInt32();
            frameCount = reader.ReadInt32();

            if(reader.ReadInt32() != 0)
            {
                paths = new Vector3[frameCount];
                for (int i = 0; i < frameCount; i++)
                    paths[i] = reader.ReadVector3();
            }

            bones = new Bone[boneCount];
            int length = -1;
            for (int i = 0; i < boneCount; i++)
            {
                Bone b = bones[i] = new Bone();
                length = reader.ReadInt32();
                char[] nameChars = reader.ReadChars(length);
                Array.Resize(ref nameChars, length - 1);//remove the null term here

                b.name = new string(nameChars);
                //b.transform = reader.ReadMatrix4x4(true); no trans is read here i guess
                b.inverseTransform = reader.ReadMatrix4x4(true);
                b.localTransform = reader.ReadMatrix4x4(true);
                b.parentId = reader.ReadInt32();
            }

            int aniCount = reader.ReadInt32();// global number of anim keys
            animations = new TMAnimation[aniCount];
            attributes = new MotionAttribute[frameCount];
            frames = new BoneFrame[boneCount];

            int frame = 0;
            int aniIndex = 0;
            TMAnimation ani = animations[0];

            for(int i = 0; i < boneCount; i++)
            {
                frame = reader.ReadInt32();
                if(frame == 1)
                {
                    for(int k = 0; k < frameCount; k++)
                    {
                        TMAnimation tm = animations[aniIndex++] = new TMAnimation();

                        tm.rotation = reader.ReadQuaternion();
                        tm.position = reader.ReadVector3();
                    }
                }
                else
                {
                    // No idea what this is
                    BoneFrame bf = frames[i] = new BoneFrame();
                    bf.frames = null;
                    bf.transform = reader.ReadMatrix4x4(true);
                }
            }

            attributes = new MotionAttribute[frameCount];
            for(int i = 0; i < frameCount; i++)
            {
                MotionAttribute ma = attributes[i] = new MotionAttribute();
                ma.type = reader.ReadUInt32();
                ma.soundId = reader.ReadInt32();
                ma.frame = reader.ReadSingle();
            }

            eventCount = reader.ReadInt32();
            if(eventCount > 0)
            {
                events = new Vector3[eventCount];
                for(int i = 0; i < eventCount; i++)             
                    events[i] = reader.ReadVector3();
                
            }

            reader.Close();
        }

        public override void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a character or monster's bone structure, which is then used
    /// in the animation process.
    /// </summary>
    internal class chr : FlyffFileFormat<Avatar>
    {
        /// <summary>
        /// Internal dictionary representing bone aliases and what they should be when
        /// importing humanoid avatars to be used in the Mecanim system. 
        /// </summary>
        private static Dictionary<string, string> boneAlias = new Dictionary<string, string>
        {
            { "","" }
        };

        //-------------------------------------------------------------------------------//
        // Bone File Structure                                                           //
        //-------------------------------------------------------------------------------//
        public int version;

        public int id;
        public int boneCount;

        public class Bone
        {
            public int parentId;
            public string name;
            public Matrix4x4 localTransform;
            public Matrix4x4 transform;
            public Matrix4x4 inverseTransform;
        }

        public Bone[] bones;// new Bone[boneCount]
        public bool sendVS;

        public Matrix4x4 localRightHand;
        public Matrix4x4 localLeftHand; // if (version == 7)
        public Matrix4x4 localShield;
        public Matrix4x4 localKnuckle;
        public Vector3[] events; // new Vector3[8], if (version >= 5)
        public int[] eventParentIds; // new Vector3[8]
        //-------------------------------------------------------------------------------//
        
        public chr(string filePath) : base(filePath)
        {}

        public override Avatar GetAssetObject()
        {
            throw new NotImplementedException();
        }

        public override void Load(string filePath)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            version = reader.ReadInt32();
            if(version < 4) throw new Exception(string.Format("Version of {0} is no longer supported.", filePath));

            id = reader.ReadInt32();
            boneCount = reader.ReadInt32();

            int length = -1;
            bones = new Bone[boneCount];
            for(int i = 0; i < boneCount; i++)
            {
                Bone b = bones[i] = new Bone();
                length = reader.ReadInt32();
                char[] nameChars = reader.ReadChars(length);
                Array.Resize(ref nameChars, length - 1);//remove the null term here

                b.name = new string(nameChars);
                b.transform = reader.ReadMatrix4x4(true);
                b.inverseTransform = reader.ReadMatrix4x4(true);
                b.localTransform = reader.ReadMatrix4x4(true);
                b.parentId = reader.ReadInt32();
            }

            sendVS = reader.ReadInt32() != 0;

            localRightHand = reader.ReadMatrix4x4(true);
            localShield = reader.ReadMatrix4x4(true);
            localKnuckle = reader.ReadMatrix4x4(true);

            if(version >= 5)
            {
                events = new Vector3[8];
                eventParentIds = new int[8];

                // (loop to 4 if version < 6) OR (loop to 8 if version >= 6)
                for (int i = 0; (i < 4 && version < 6) || (i < 8 && version >= 6); i++)
                {
                    events[i] = reader.ReadVector3();
                    eventParentIds[i] = reader.ReadInt32();
                }
            }

            if(version == 7)
            {
                localLeftHand = reader.ReadMatrix4x4(true);
            }

            reader.Close();
        }

        public override void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}