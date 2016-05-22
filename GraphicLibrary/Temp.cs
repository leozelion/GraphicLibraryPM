/*float a = 10;
float b = 10;
float c = 10;

int stacks_x = 20;
int stacks_z = 20;

vertexes = new PositionedColoredVertex[stacks_x * stacks_z];
                
float phi, theta;            
float dphi = (float)(Math.PI / (stacks + 1));
float dtheta = (float)(Math.PI * 2 / slices);            
float x, y, z, sc;            
int index = 0;            
for (int stack = 1; stack <= stacks; stack++)            
{
    phi = (float)(Math.PI / 2 - stack * dphi);                
    y = radius * (float)Math.Sin(phi);                
    sc = radius * (float)Math.Cos(phi);                
    for (int slice = 0; slice < slices; slice++)                
    {                    
        theta = slice * dtheta;                    
        x = sc * (float)Math.Cos(theta);                    
        z = sc * (float)Math.Sin(theta);                    
                        
        vertexes[index++] = new PositionedColoredVertex(new Vector3(x, y, z), Color.White.ToArgb());
    }            
}            
                
indices = new int[slices * (stacks - 1) * 6];            
index = 0;            
for (int stack = 0; stack < (stacks - 1); stack++)            
{                
    for (int slice = 0; slice < slices; slice++)                
    {
        indices[index++] = (stack + 0) * slices + slice;
        indices[index++] = (stack + 1) * slices + slice;
        indices[index++] = (stack + 0) * slices + (slice + 1) % slices;

        indices[index++] = (stack + 0) * slices + (slice + 1) % slices;
        indices[index++] = (stack + 1) * slices + slice;
        indices[index++] = (stack + 1) * slices + (slice + 1) % slices;                
    }            
}

if (vertexBuffer != null)
    vertexBuffer.Dispose();
if (indexBuffer != null)
    indexBuffer.Dispose();

vertexBuffer = new VertexBuffer(device, PositionedColoredVertex.SizeInBytes * vertexes.Length, Usage.WriteOnly, VertexFormat.None, Pool.Default);
indexBuffer = new IndexBuffer(device, indices.Length * sizeof(int), Usage.WriteOnly, Pool.Default, false); 

DataStream data_stream = vertexBuffer.Lock(0, 0, LockFlags.None);
data_stream.WriteRange(vertexes);
vertexBuffer.Unlock();
                
data_stream = indexBuffer.Lock(0, 0, LockFlags.None);
data_stream.WriteRange(indices);
indexBuffer.Unlock();*/



/*int stacks = 20;
int slices = 20;

float radius = 10;

vertexes = new PositionedColoredVertex[stacks * slices];
                
float phi, theta;            
float dphi = (float)(Math.PI / (stacks + 1));
float dtheta = (float)(Math.PI * 2 / slices);            
float x, y, z, sc;            
int index = 0;            
for (int stack = 1; stack <= stacks; stack++)            
{
    phi = (float)(Math.PI / 2 - stack * dphi);                
    y = radius * (float)Math.Sin(phi);                
    sc = radius * (float)Math.Cos(phi);                
    for (int slice = 0; slice < slices; slice++)                
    {                    
        theta = slice * dtheta;                    
        x = sc * (float)Math.Cos(theta);                    
        z = sc * (float)Math.Sin(theta);                    
                        
        vertexes[index++] = new PositionedColoredVertex(new Vector3(x, y, z), Color.White.ToArgb());
    }            
}            
                
indices = new int[slices * (stacks - 1) * 6];            
index = 0;            
for (int stack = 0; stack < (stacks - 1); stack++)            
{                
    for (int slice = 0; slice < slices; slice++)                
    {
        indices[index++] = (stack + 0) * slices + slice;
        indices[index++] = (stack + 1) * slices + slice;
        indices[index++] = (stack + 0) * slices + (slice + 1) % slices;

        indices[index++] = (stack + 0) * slices + (slice + 1) % slices;
        indices[index++] = (stack + 1) * slices + slice;
        indices[index++] = (stack + 1) * slices + (slice + 1) % slices;                
    }            
}

if (vertexBuffer != null)
    vertexBuffer.Dispose();
if (indexBuffer != null)
    indexBuffer.Dispose();

vertexBuffer = new VertexBuffer(device, PositionedColoredVertex.SizeInBytes * vertexes.Length, Usage.WriteOnly, VertexFormat.None, Pool.Default);
indexBuffer = new IndexBuffer(device, indices.Length * sizeof(int), Usage.WriteOnly, Pool.Default, false); 

DataStream data_stream = vertexBuffer.Lock(0, 0, LockFlags.None);
data_stream.WriteRange(vertexes);
vertexBuffer.Unlock();
                
data_stream = indexBuffer.Lock(0, 0, LockFlags.None);
data_stream.WriteRange(indices);
indexBuffer.Unlock();*/


/*if (vertexBuffer != null)
    vertexBuffer.Dispose();
vertexBuffer = new VertexBuffer(device, 8 * Marshal.SizeOf(typeof(PositionedColoredVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);
vertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(new[] {
        new PositionedColoredVertex( new Vector3(-3f,  3f, -3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3( 3f,  3f, -3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3(-3f, -3f, -3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3( 3f, -3f, -3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3(-3f,  3f,  3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3( 3f,  3f,  3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3(-3f, -3f,  3f), Color.White.ToArgb() ),
        new PositionedColoredVertex( new Vector3( 3f, -3f,  3f), Color.White.ToArgb() ),
    });

vertexBuffer.Unlock();



if (indexBuffer != null)
    indexBuffer.Dispose();

indexBuffer = new IndexBuffer(device, 3 * 12 * sizeof(short), Usage.WriteOnly, Pool.Default, true);
indexBuffer.Lock(0, 0, LockFlags.None).WriteRange(new short[] {
     0, 1, 2,    // side 1
    2, 1, 3,
    4, 0, 6,    // side 2
    6, 0, 2,
    7, 5, 6,    // side 3
    6, 5, 4,
    3, 1, 7,    // side 4
    7, 1, 5,
    4, 5, 0,    // side 5
    0, 5, 1,
    3, 7, 2,    // side 6
    2, 7, 6,
});
indexBuffer.Unlock();*/


                        //x = (radius1 - radius2 + radius2 * (float)Math.Cos(theta)) * (float)Math.Cos(phi);
                        //y = (radius1 - radius2 + radius2 * (float)Math.Cos(theta)) * (float)Math.Sin(phi);
                        //z = radius2 * (float)Math.Sin(theta);




                        /*if ((slice == 0) || (stack == 0) || (slice == slices - 1) || (stack == stacks - 1))
                        {
                            //Vector3 normal = new Vector3(x, y, 0);
                            //normal.Normalize();
                            //Matrix rotation = Matrix.RotationAxis(new Vector3(0, 1, 0), -theta);
                            //float temp1 = normal.X;
                            //float temp2 = normal.Y;
                            //float temp3 = normal.Z;
                            //normal.X = rotation.M11 * temp1 + rotation.M21 * temp2 + rotation.M31 * temp3;
                            //normal.Y = rotation.M12 * temp1 + rotation.M22 * temp2 + rotation.M32 * temp3;
                            //normal.Z = rotation.M13 * temp1 + rotation.M23 * temp2 + rotation.M33 * temp3;
                            //normal.Normalize();

                            Vector3 normal = new Vector3(x, y, z) - second_center;
                            normal.Normalize();
                            
                            vertexesMain[index] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), -normal);
                            vertexesMain[offset + index] = new PositionedColoredNormalVertex(new Vector3(x, y, z) - normal * h / 2 + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), normal);
                        }
                        else
                        {
                            Vector3 normal = new Vector3(x, y, z) - second_center;
                            normal.Normalize();

                            //Vector3 normal = new Vector3(x, y, 0);
                            //normal.Normalize();
                            //Matrix rotation = Matrix.RotationAxis(new Vector3(0, 1, 0), -theta);
                            //float temp1 = normal.X;
                            //float temp2 = normal.Y;
                            //float temp3 = normal.Z;
                            //normal.X = rotation.M11 * temp1 + rotation.M21 * temp2 + rotation.M31 * temp3;
                            //normal.Y = rotation.M12 * temp1 + rotation.M22 * temp2 + rotation.M32 * temp3;
                            //normal.Z = rotation.M13 * temp1 + rotation.M23 * temp2 + rotation.M33 * temp3;
                            

                            //Vector3 normal = new Vector3(sc, y, 0);
                            //Matrix rotation = Matrix.RotationAxis(new Vector3(0, 1, 0), -theta);
                            //float temp1 = normal.X;
                            //float temp2 = normal.Y;
                            //float temp3 = normal.Z;
                            //normal.X = rotation.M11 * temp1 + rotation.M21 * temp2 + rotation.M31 * temp3;
                            //normal.Y = rotation.M12 * temp1 + rotation.M22 * temp2 + rotation.M32 * temp3;
                            //normal.Z = rotation.M13 * temp1 + rotation.M23 * temp2 + rotation.M33 * temp3;
                            
                            //vertexes[index] = new PositionedColoredNormalVertex(new Vector3(x - radius1 + h / 2, y, z), Color.White.ToArgb(), -normal);
                            //vertexes[offset + index] = new PositionedColoredNormalVertex(new Vector3(x - radius1 - h / 2, y, z), Color.Wheat.ToArgb(), normal);
                            vertexesMain[index] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), -normal);
                            vertexesMain[offset + index] = new PositionedColoredNormalVertex(new Vector3(x, y, z) - normal * h / 2 + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), normal);
                        }
                        index++;*/
//x = radius2 * (float)Math.Cos(theta) + sc - radius2;
//z = radius2 * (float)Math.Sin(theta);



/*int indexEdges = 0;
int offsetEdges = (stacks + slices) * 2;
int stack1 = 0;
for (int slice = 0; slice < (slices - 1); slice++)
{
    vertexesEdges[indexEdges] = vertexesMain[stack1 * slices + slice];
    vertexesEdges[offsetEdges + indexEdges] = vertexesMain[offsetMain + stack1 * slices + slice];
    indexEdges++;
}*/
/*stack1 = stacks - 1;
for (int slice = 0; slice < (slices - 1); slice++)
{
    indices[index++] = (stack1 + 0) * slices + (slice + 0);
    indices[index++] = (stack1 + 0) * slices + (slice + 1);
    indices[index++] = (stack1 + 0) * slices + (slice + 0) + offset;

    indices[index++] = (stack1 + 0) * slices + (slice + 1);
    indices[index++] = (stack1 + 0) * slices + (slice + 0) + offset;
    indices[index++] = (stack1 + 0) * slices + (slice + 1) + offset;
}
                
int slice1 = 0;
for (int stack = 0; stack < (stacks - 1); stack++)
{
    indices[index++] = (stack + 0) * slices + (slice1 + 0);
    indices[index++] = (stack + 1) * slices + (slice1 + 0);
    indices[index++] = (stack + 0) * slices + (slice1 + 0) + offset;

    indices[index++] = (stack + 1) * slices + (slice1 + 0);
    indices[index++] = (stack + 0) * slices + (slice1 + 0) + offset;
    indices[index++] = (stack + 1) * slices + (slice1 + 0) + offset;
}
slice1 = slices - 1;
for (int stack = 0; stack < (stacks - 1); stack++)
{
    indices[index++] = (stack + 0) * slices + (slice1 + 0);
    indices[index++] = (stack + 1) * slices + (slice1 + 0);
    indices[index++] = (stack + 0) * slices + (slice1 + 0) + offset;

    indices[index++] = (stack + 1) * slices + (slice1 + 0);
    indices[index++] = (stack + 0) * slices + (slice1 + 0) + offset;
    indices[index++] = (stack + 1) * slices + (slice1 + 0) + offset;
}*/







        //квад верхнего слоя
        /*PositionedTexturedVertex[] quadLayerTop = new PositionedTexturedVertex[6];
        VertexBuffer vbQuadLayerTop;

        //квад экрана
        static PositionedTexturedVertex[] quadScreen;
        
        //верхний слой
        Texture layerTop;*/




                /*if (quadScreen == null)
                {
                    quadScreen = new PositionedTexturedVertex[6];
                    quadScreen[0] = new PositionedTexturedVertex(new Vector3(-1, 1, 1), new Vector2(0, 0));
                    quadScreen[1] = new PositionedTexturedVertex(new Vector3(1, 1, 1), new Vector2(1, 0));
                    quadScreen[2] = new PositionedTexturedVertex(new Vector3(1, -1, 1), new Vector2(1, 1));
                    quadScreen[3] = quadScreen[0];
                    quadScreen[4] = quadScreen[2];
                    quadScreen[5] = new PositionedTexturedVertex(new Vector3(-1, -1, 1), new Vector2(0, 1));
                    quadScreen.CopyTo(quadLayerTop, 0);
                }
                    
                vbQuadLayerTop = new VertexBuffer(device, PositionedTexturedVertex.SizeInBytes * 6, Usage.WriteOnly, VertexFormat.None, Pool.Default);
                DataStream data_stream = vbQuadLayerTop.Lock(0, 0, LockFlags.None);
                data_stream.WriteRange(quadLayerTop);
                vbQuadLayerTop.Unlock();

                //формирование верхнего слоя
                layerTop = new Texture(device, ownerControl.ClientSize.Width, ownerControl.ClientSize.Height, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);*/


        //очистить верхний слой
        /*public ReturnCode ClearLayerTop()
        {
            try
            {
                DataRectangle dr = layerTop.LockRectangle(0, LockFlags.None);
                for (int l = 0; l < dr.Data.Length; l++)
                    dr.Data.WriteByte(0);
                layerTop.UnlockRectangle(0);

                return ReturnCode.Success;
            }
            catch
            {
                return ReturnCode.Fail;
            }
        }

        //возврат верхнего слоя
        public IntPtr GetLayerTopHDC()
        {
            try
            {
                return layerTop.GetSurfaceLevel(0).GetDC();
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
        public ReturnCode ReleaseLayerTopHDC(IntPtr hDC)
        {
            try
            {
                layerTop.GetSurfaceLevel(0).ReleaseDC(hDC);

                return ReturnCode.Success;
            }
            catch
            {
                return ReturnCode.Fail;
            }
        }
        
        //задать смещение верхнего слоя
        public ReturnCode SetLayerTopShift(int dx, int dy)
        {
            try
            {
                for (int i = 0; i < quadLayerTop.Length; i++)
                    quadLayerTop[i].position = quadScreen[i].position + new Vector3(2.0f * dx / ownerControl.ClientSize.Width, -2.0f * dy / ownerControl.ClientSize.Height, 0);
                DataStream stream = vbQuadLayerTop.Lock(0, 0, LockFlags.Discard);
                stream.WriteRange(quadLayerTop);
                vbQuadLayerTop.Unlock();

                return ReturnCode.Success;
            }
            catch
            {
                return ReturnCode.Fail;
            }
        }
        public ReturnCode AddLayerTopShift(int dx, int dy)
        {
            try
            {
                for (int i = 0; i < quadLayerTop.Length; i++)
                    quadLayerTop[i].position += new Vector3(2.0f * dx / ownerControl.ClientSize.Width, -2.0f * dy / ownerControl.ClientSize.Height, 0);
                DataStream stream = vbQuadLayerTop.Lock(0, 0, LockFlags.Discard);
                stream.WriteRange(quadLayerTop);
                vbQuadLayerTop.Unlock();

                return ReturnCode.Success;
            }
            catch
            {
                return ReturnCode.Fail;
            }
        }*/



//Light light0 = new Light();
//light0.Position = new Vector3(10, 0, 0);
//light0.Direction = new Vector3(0, 0, 1);
//light0.Type = LightType.Directional;
//light0.Diffuse = new Color4(Color.Green.ToArgb());
//device.SetLight(0, light0);



/*#region Прогиб
            float[][] progibs = new float[stacks][];
            for (int i = 0; i < progibs.Length; i++)
            {
                progibs[i] = new float[slices];
            }

            float radius_progiba = 8.5f;
            float radius_progiba_stack_slice = (radius_progiba / a) * stacks;
            int stack_center = stacks / 2;
            int slice_center = slices / 2;
            for (int slice = 0; slice < slices; slice++)
                for (int stack = 0; stack < stacks; stack++)
                {
                    if (Math.Sqrt((slice - slice_center) * (slice - slice_center) + (stack - stack_center) * (stack - stack_center)) <= radius_progiba_stack_slice)
                    {
                        float v1 = (slice - slice_center) * (a / slices);
                        float v2 = (stack - stack_center) * (b / stacks);
                        //if (value < 0.5f * radius_progiba)
                        value = 0.5f * value;
                        float v3 = radius_progiba * radius_progiba - v1 * v1 - v2 * v2;
                        if (v3 < 0) v3 = 0;
                        float value = (float)Math.Sqrt(v3);
                        progibs[stack][slice] = value;
                        //progibs[stack][slice] = 0;
                    }
                }
            #endregion*/



/*float y = ra * (float)Math.Sin(phi);
float sc = rb * (float)Math.Cos(phi);

Vector3 radius1_proj_parallel_normalized = new Vector3(sc, y, 0);
radius1_proj_parallel_normalized.Normalize();
float radius1_proj_parallel = radius2 * (float)Math.Cos(theta);
float radius1_proj_perpendicular = radius2 * (float)Math.Sin(theta);

float x = radius1_proj_parallel * (float)Math.Cos(phi) + sc - radius2 * (float)Math.Cos(phi);
y = y - (radius2 * (float)Math.Sin(phi) - radius1_proj_parallel * (float)Math.Sin(phi));
float z = radius1_proj_perpendicular;

Vector3 second_center = radius1_proj_parallel_normalized * (radius1 - radius2);

Vector3 normal = new Vector3(x, y, z) - second_center;
normal.Normalize();

if ((slice == 0) || (stack == 0) || (slice == slices - 1) || (stack == stacks - 1))
{
    //vertexesMain[indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0), Color.Red.ToArgb(), -normal);
    vertexesMain[indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * hij[stack, slice] / 2 + new Vector3(-radius1, 0, 0), Color.Red.ToArgb(), -normal);
    vertexesMain[offsetMain + indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) - normal * h / 2 + new Vector3(-radius1, 0, 0), Color.Red.ToArgb(), normal);
    vertexesNormals[2 * indexMain + 0] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0), Color.Green.ToArgb(), Vector3.Zero);
    vertexesNormals[2 * indexMain + 1] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0) + normal * 10, Color.Green.ToArgb(), Vector3.Zero);
}
else
{
    //float add = progibs[stack][slice];// 5 * (float)rand.NextDouble();
    float add = 0;// 5 * (float)rand.NextDouble();
    //vertexesMain[indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * (add + h / 2) + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), -normal);
    vertexesMain[indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * (add + hij[stack, slice] / 2) + new Vector3(-radius1, 0, 0), Color.White.ToArgb(), -normal);
    vertexesMain[offsetMain + indexMain] = new PositionedColoredNormalVertex(new Vector3(x, y, z) - normal * (h / 2 - add) + new Vector3(-radius1, 0, 0), Color.Wheat.ToArgb(), normal);
    vertexesNormals[2 * indexMain + 0] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0), Color.Green.ToArgb(), Vector3.Zero);
    vertexesNormals[2 * indexMain + 1] = new PositionedColoredNormalVertex(new Vector3(x, y, z) + normal * h / 2 + new Vector3(-radius1, 0, 0) + normal * 10, Color.Green.ToArgb(), Vector3.Zero);
}*/
//vertexesAxisX = new PositionedColoredNormalVertex[slices * 2];
//vertexesAxisY = new PositionedColoredNormalVertex[stacks * 2];
//vertexesAxisZ = new PositionedColoredNormalVertex[2];
//int indexAxisX = 0;
//int indexAxisY = 0;
//if (stack == 0)
//vertexesAxisX[indexAxisX++] = new PositionedColoredNormalVertex(new Vector3(x - r - R, y, z), Color.Red.ToArgb(), Vector3.UnitY);
//if (slice == 0)
//vertexesAxisY[indexAxisY++] = new PositionedColoredNormalVertex(new Vector3(x - r - R, y, z), Color.Yellow.ToArgb(), Vector3.UnitY);


//device.DrawUserPrimitives(PrimitiveType.PointList, vertexesAxisX.Length, vertexesAxisX);
//device.DrawUserPrimitives(PrimitiveType.PointList, vertexesAxisY.Length, vertexesAxisY);
