#ifndef UNITY_IMPOSTOR_COMMON
#define UNITY_IMPOSTOR_COMMON

#if SHADER_API_MOBILE || SHADER_API_GLES || SHADER_API_GLES3
#pragma warning (disable : 3205) // conversion of larger type to smaller
#endif


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"