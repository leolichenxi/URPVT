namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Shader Pass Info
    /// </summary>
    public struct PassID
    {
        public int Pass;
        public int ShadowCasterPass;
        public int ImpostorPass;
        public int PreZPass;
        public int AfterPreZPass;

        public bool HasPass => Pass != -1;
        public bool HasImpostorPass => ImpostorPass != -1;
        public bool HasShadowCasterPass => ShadowCasterPass != -1;
        public bool HasPreZPass => PreZPass != -1;
        public bool HasAfterPreZPass => AfterPreZPass != -1;

        public void ReadPassFromMaterial(Material material, bool findImpostor)
        {
            Pass = 0;
            ShadowCasterPass = -1;
            ImpostorPass = -1;
            PreZPass = -1;
            AfterPreZPass = -1;
            
            if (material != null)
            {
                Pass = material.FindPass(ImpostorConst.PassAfterZ);
                if (Pass == -1)
                {
                    Pass = material.FindPass(ImpostorConst.PassForwardLit);
                }
                if (Pass == -1)
                {
                    Pass = material.FindPass(ImpostorConst.PassMain);
                }
                if (Pass == -1)
                {
                    Pass = 0;
                }

                ShadowCasterPass = material.FindPass(ImpostorConst.PassShadowCaster);
                if (findImpostor)
                {
                    ImpostorPass = material.FindPass(ImpostorConst.PassImpostor);
                    if (ImpostorPass > 0)
                    {
                        Pass = ImpostorPass;
                    }
                }
                else
                {
                    PreZPass = material.FindPass(ImpostorConst.PassPreZ);
                    AfterPreZPass = material.FindPass(ImpostorConst.PassAfterZ);
                }
            }
        }
    }
}