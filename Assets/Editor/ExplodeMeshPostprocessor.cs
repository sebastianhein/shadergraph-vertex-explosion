using UnityEngine;
using UnityEditor;

public class ExplodeMeshPostprocessor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        // TODO: check for explosion tag
        var filters = g.GetComponentsInChildren<MeshFilter>();
        g.name = g.name + "_filtered";

        for (var i=0; i<filters.Length; i++) {
            var f = filters[i];
            PerpareMeshForExplosion(f.sharedMesh);
        }
    }

    void PerpareMeshForExplosion(Mesh mesh)
    {
        Debug.Log("unwelding vertices: " + mesh.name);

        Vector3[] verts   = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uv      = mesh.uv;
        int[] indices     = mesh.GetIndices(0);

        var count = indices.Length;
        Vector3[] targetVerts   = new Vector3[count];
        Vector3[] targetNormals = new Vector3[count];
        Vector2[] targetUv      = new Vector2[count];

        // store face normal in UV2 and UV3
        Vector2[] targetUv1     = new Vector2[count];
        Vector2[] targetUv2     = new Vector2[count];
        Vector2[] targetUv3     = new Vector2[count];

        int[] targetIndices     = new int[count];

        // unweld vertices, aka create individial vertices for each triangle
        for (int i=0; i<indices.Length; i++) {
            targetVerts[i] = verts[indices[i]];
            targetUv[i] = uv[indices[i]];
            targetNormals[i] = normals[indices[i]];
            targetIndices[i] = i;
        }
  
        for (int i=0; i<indices.Length; i+=3) {
            // calculate face normals
            Vector3 faceNormal = (targetNormals[i] + targetNormals[i+1] + targetNormals[i+2]);
            faceNormal.Scale(new Vector3(1.0f/3.0f, 1.0f/3.0f, 1.0f/3.0f));

            Vector2 blendUV = (targetUv[i] + targetUv[i+1] + targetUv[i+2]);
            blendUV.Scale(new Vector2(1.0f/3.0f, 1.0f/3.0f));

            Vector2 uv1 = new Vector2(faceNormal.x, faceNormal.y);
            Vector2 uv2 = new Vector2(faceNormal.z, 1);
            Vector2 uv3 = new Vector2(faceNormal.z, 1);            
       
            targetUv1[i] = uv1;
            targetUv1[i+1] = uv1;
            targetUv1[i+2] = uv1;

            targetUv2[i] = uv2;
            targetUv2[i+1] = uv2;
            targetUv2[i+2] = uv2;

            // face centric uv map to allow delayed explosion based on a gradient map
            targetUv3[i] = blendUV;
            targetUv3[i+1] = blendUV;
            targetUv3[i+2] = blendUV;        
        }

        mesh.Clear(false);
        mesh.SetVertices(targetVerts);
        mesh.SetUVs(0, targetUv);
        mesh.SetUVs(1, targetUv1);
        mesh.SetUVs(2, targetUv2);
        mesh.SetUVs(3, targetUv3);
        
        mesh.SetNormals(targetNormals);
        mesh.SetIndices(targetIndices, MeshTopology.Triangles, 0);
        mesh.MarkModified();
    }
}