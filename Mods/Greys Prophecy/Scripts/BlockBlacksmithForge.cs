using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BlockBlacksmithForge : BlockForge
{
    protected override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        var chunk = _world.GetChunkFromWorldPos(_blockPos) as Chunk;

        if (chunk != null)
        {
            var blockEntity = chunk.GetBlockEntity(_blockPos);

            if (blockEntity != null && blockEntity.bHasTransform)
            {
                var forgeLights = blockEntity.transform.FindInChilds("fl_forge_orange");

                if(forgeLights == null)
                {
                    return;
                }

                forgeLights.gameObject.SetActive(_blockValue.meta != 0);
            }
        }
    }
}
