﻿using Maple.MonoGameAssistant.AndroidJNI.Context;
using Maple.MonoGameAssistant.AndroidJNI.JNI.Primitive;
using Maple.MonoGameAssistant.AndroidJNI.JNI.Reference;
using System.Runtime.CompilerServices;
namespace Maple.MonoGameAssistant.AndroidCore.Api
{
    public readonly struct VirtualActionApiCallbackInstance(JLOCAL<JOBJECT> instance, VirtualActionApiCallbackReference reference)
        : IJavaClassLocalInstance<VirtualActionApiCallbackReference>
    {

        public JLOCAL<JOBJECT> JLocal => instance;

        public JOBJECT Instance => JLocal.Value;

        public VirtualActionApiCallbackReference Reference => reference;


        public static bool TryCreate(in JniEnvironmentContext jniEnvironmentContext, JWEAK<JOBJECT> ptr, out VirtualActionApiCallbackInstance callbackInstance)
        {
            Unsafe.SkipInit(out callbackInstance);
            if (jniEnvironmentContext.JNI_ENV.TryWeak2Local(ptr, out var localObj))
            {
                var metadata = VirtualActionApiCallbackMetadata.CreateMetadata(jniEnvironmentContext, localObj);
                var classRef = VirtualActionApiCallbackReference.CreateReference(jniEnvironmentContext, metadata);
                callbackInstance = new VirtualActionApiCallbackInstance(localObj, classRef);
                return true;
            }
            return false;
        }



        public JBOOLEAN None(JSTRING json) => this.Reference.None(this.Instance, json);

        public JBOOLEAN EnumImages(JSTRING json) => this.Reference.EnumImages(this.Instance, json);
        public JBOOLEAN EnumClasses(JSTRING json) => this.Reference.EnumClasses(this.Instance, json);
        public JBOOLEAN EnumObjects(JSTRING json) => this.Reference.EnumObjects(this.Instance, json);
        public JBOOLEAN EnumClassDetail(JSTRING json) => this.Reference.EnumClassDetail(this.Instance, json);


        public JBOOLEAN INFO(JSTRING json) => this.Reference.INFO(this.Instance, json);
        public JBOOLEAN LoadResource(JSTRING json) => this.Reference.LoadResource(this.Instance, json);
        public JBOOLEAN GetListCurrencyDisplay(JSTRING json) => this.Reference.GetListCurrencyDisplay(this.Instance, json);
        public JBOOLEAN GetCurrencyInfo(JSTRING json) => this.Reference.GetCurrencyInfo(this.Instance, json);
        public JBOOLEAN UpdateCurrencyInfo(JSTRING json) => this.Reference.UpdateCurrencyInfo(this.Instance, json);
        public JBOOLEAN GetListInventoryDisplay(JSTRING json) => this.Reference.GetListInventoryDisplay(this.Instance, json);
        public JBOOLEAN GetInventoryInfo(JSTRING json) => this.Reference.GetInventoryInfo(this.Instance, json);
        public JBOOLEAN UpdateInventoryInfo(JSTRING json) => this.Reference.UpdateInventoryInfo(this.Instance, json);
        public JBOOLEAN GetListCharacterDisplay(JSTRING json) => this.Reference.GetListCharacterDisplay(this.Instance, json);
        public JBOOLEAN GetCharacterStatus(JSTRING json) => this.Reference.GetCharacterStatus(this.Instance, json);
        public JBOOLEAN UpdateCharacterStatus(JSTRING json) => this.Reference.UpdateCharacterStatus(this.Instance, json);
        public JBOOLEAN GetCharacterEquipment(JSTRING json) => this.Reference.GetCharacterEquipment(this.Instance, json);
        public JBOOLEAN UpdateCharacterEquipment(JSTRING json) => this.Reference.UpdateCharacterEquipment(this.Instance, json);
        public JBOOLEAN GetCharacterSkill(JSTRING json) => this.Reference.GetCharacterSkill(this.Instance, json);
        public JBOOLEAN UpdateCharacterSkill(JSTRING json) => this.Reference.UpdateCharacterSkill(this.Instance, json);
        public JBOOLEAN GetListMonsterDisplay(JSTRING json) => this.Reference.GetListMonsterDisplay(this.Instance, json);
        public JBOOLEAN AddMonsterMember(JSTRING json) => this.Reference.AddMonsterMember(this.Instance, json);
        public JBOOLEAN GetListSkillDisplay(JSTRING json) => this.Reference.GetListSkillDisplay(this.Instance, json);
        public JBOOLEAN AddSkillDisplay(JSTRING json) => this.Reference.AddSkillDisplay(this.Instance, json);
        public JBOOLEAN GetListSwitchDisplay(JSTRING json) => this.Reference.GetListSwitchDisplay(this.Instance, json);
        public JBOOLEAN UpdateSwitchDisplay(JSTRING json) => this.Reference.UpdateSwitchDisplay(this.Instance, json);


    }

}
