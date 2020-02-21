using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

public class MinEventActionPlayParticleEffect : MinEventActionBase
{
    private GameObject _particleEffect;
    private float _timeToLive = 5f;

    public override void Execute(MinEventParams _params)
    {
        var gameObj = UnityEngine.Object.Instantiate(_particleEffect, _params.Position, _params.Transform.rotation);

        ParticleSystem[] particleSystems = gameObj.GetComponentsInChildren<ParticleSystem>();

        if(particleSystems != null && particleSystems.Any())
        {
            foreach(var ps in particleSystems)
            {
                ps.Stop();
                var main = ps.main;
                main.duration = 10000f;

                ps.Play();
            }
        }

        UnityEngine.Object.Destroy(gameObj, _timeToLive);
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        var xmlAttribute = base.ParseXmlAttribute(_attribute);

        if (!xmlAttribute)
        {
            if (_attribute.Name == "particle")
            {
                _particleEffect = DataLoader.LoadAsset<GameObject>(_attribute.Value);
                return true;
            }

            if(_attribute.Name == "ttl")
            {
                float.TryParse(_attribute.Value, out _timeToLive);
                return true;
            }
        }

        return xmlAttribute;
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self != null && _particleEffect != null;
    }
}
