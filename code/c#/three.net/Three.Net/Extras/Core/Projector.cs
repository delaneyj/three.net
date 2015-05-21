using Three.Net.Cameras;
using Three.Net.Math;

namespace Three.Net.Extras.Core
{
    public class Projector
    {
        static Matrix4 viewProjectionMatrix;

        public static Vector3 ProjectVector(Vector3 vector, Camera camera)
        {
            camera.matrixWorldInverse = Matrix4.GetInverse(camera.matrixWorld);
            viewProjectionMatrix.MultiplyMatrices(camera.projectionMatrix, camera.matrixWorldInverse);
            var v = vector;
            v.ApplyProjection(viewProjectionMatrix);
            return v;
        }

        public static Vector3 UnprojectVector(Vector3 vector, Matrix4 projectionMatrix, Matrix4 matrixWorld)
        {
            var projectionMatrixInverse = Matrix4.GetInverse(projectionMatrix);
            viewProjectionMatrix.MultiplyMatrices(matrixWorld, projectionMatrixInverse);
            var v = vector;
            v.ApplyProjection(viewProjectionMatrix);
            return v;
        }

        public static Raycaster PickingRay(Vector3 vector, Camera camera)
        {
            // set two vectors with opposing z values
            vector.z = -1;
            var end = new Vector3(vector.x, vector.y, 1);
            var start = UnprojectVector(vector, camera.projectionMatrix, camera.matrixWorld);
            end = UnprojectVector(end, camera.projectionMatrix, camera.matrixWorld);

            // find direction from vector to end
            end.Subtract(start);
            end.Normalize();
            return new Raycaster(start, end);
        }
    }
}
