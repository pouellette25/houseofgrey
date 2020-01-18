using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Text;
using UnityEngine;
using SDX.Core;
using SDX.Compiler;
using SDX.Payload;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
//using System.Reflection;


public class BackpackButtonsPatcher : IPatcherMod
{
    public bool Patch(ModuleDefinition module)
    {
        FieldDefinition field;
        TypeDefinition gameClass;
        MethodDefinition gameMethod;
        TypeReference voidTypeRef = module.TypeSystem.Void;

        Console.WriteLine("==CreatePlayerAction Patcher===");

        // Making required things public
        var gm = module.Types.First(d => d.Name == "PlayerActionSet");
        var method = gm.Methods.First(d => d.Name == "CreatePlayerAction");
        SetMethodToPublic(method);
		
        // Making required things public
        var gmm = module.Types.First(d => d.Name == "ItemActionEntryUse");
        var method2 = gmm.Methods.First(d => d.Name == "switchBack");
        SetMethodToPublic(method2);

        // Making required things public
        gm = module.Types.First(d => d.Name == "PlayerActionsLocal");
        field = gm.Fields.First(d => d.Name == "InventoryActions");
        SetFieldToPublic(field);
        field.IsInitOnly = false;
		
        // Making required things public
        gm = module.Types.First(d => d.Name == "Inventory");
        field = gm.Fields.First(d => d.Name == "slots");
        SetFieldToPublic(field);
        field.IsInitOnly = false;
		
        // Making required things public
        gm = module.Types.First(d => d.Name == "Inventory");
        field = gm.Fields.First(d => d.Name == "entity");
        SetFieldToPublic(field);
        field.IsInitOnly = false;

        gm = module.Types.First(d => d.Name == "PlayerActionsLocal");
        var myaction = module.Types.First(d => d.Name == "PlayerAction");
        gm.Fields.Add(new FieldDefinition("InventorySlot10", FieldAttributes.Public, myaction));
        gm.Fields.Add(new FieldDefinition("InventorySlot11", FieldAttributes.Public, myaction));
        return true;
    }
	
    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private void SetMethodToVirtual(MethodDefinition meth)
    {
        meth.IsVirtual = true;
    }

    private void SetFieldToPublic(FieldDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private void SetMethodToPublic(MethodDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }

}

