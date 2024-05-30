using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class NewCustomPass : CustomPass
{
    public RenderTexture resultTexture;
    public ComputeShader computeShader;

    private CommandBuffer cmd;
    private int mainKernelID;
    private int clearKernelID;
    private RayTracingAccelerationStructure rtas = null;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer setupCmd)
    {
        mainKernelID = computeShader.FindKernel("CSMain");
        clearKernelID = computeShader.FindKernel("CSClear");
        RayTracingAccelerationStructure.Settings settings = new RayTracingAccelerationStructure.Settings();
        settings.rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything;
        settings.managementMode = RayTracingAccelerationStructure.ManagementMode.Automatic;
        settings.layerMask = 255;
        rtas = new RayTracingAccelerationStructure(settings);
        
    }

    protected override void Execute(CustomPassContext ctx)
    {
        cmd = ctx.cmd;
        
        // bind shader properties and add commands to command buffer
        cmd.BuildRayTracingAccelerationStructure(rtas);
        cmd.SetRayTracingAccelerationStructure(computeShader, mainKernelID, "AccelStruct", rtas);
        cmd.SetComputeTextureParam(computeShader, mainKernelID, "Result", resultTexture);
        cmd.SetComputeTextureParam(computeShader, clearKernelID, "Result", resultTexture);
        
        // finally, dispatch compute shader
        cmd.DispatchCompute(computeShader, clearKernelID, 512/8, 512/8, 1);
        cmd.DispatchCompute(computeShader, mainKernelID, 512/8, 512/8, 1);
        Debug.Log("dispatched compute");
    }

    protected override void Cleanup()
    {
        
    }
}