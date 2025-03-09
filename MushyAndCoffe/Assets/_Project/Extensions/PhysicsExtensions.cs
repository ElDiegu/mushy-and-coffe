using UnityEngine;

namespace MushyAndCoffe.Extensions
{
	public static class PhysicsExtensions
	{
		public static bool MouseRaycastFromCamera(this Physics physics, Vector3 mousePos, Camera desiredCamera, float maxDistance, LayerMask placementLayerMask, out Vector3 result) 
		{
			mousePos.z = desiredCamera.nearClipPlane;
			
			Ray ray = desiredCamera.ScreenPointToRay(mousePos);
			
			result = new Vector3();
			
			if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementLayerMask)) 
			{
				result = hit.point;
				return true;
			}
			
			return false;
		}
		
		public static bool MouseRaycastFromCamera(this Physics physics, Vector3 mousePos, Camera desiredCamera, float maxDistance, LayerMask placementLayerMask, out RaycastHit result) 
		{
			mousePos.z = desiredCamera.nearClipPlane;
			
			Ray ray = desiredCamera.ScreenPointToRay(mousePos);
			
			result = new RaycastHit();
			
			if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementLayerMask)) 
			{
				result = hit;
				return true;
			}
			
			return false;
		}
	}
}
