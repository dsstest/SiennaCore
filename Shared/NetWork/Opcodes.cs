﻿using System;

public enum Opcodes
{
    ProtocolHandshakeVersion = 0x01B7,

    ProtocolPingPong = 0x0002,
    ProtocolHandshakeCompression = 0x0019,

    ProtocolHandshakeClientKey = 0x040A,
    ProtocolHandshakeServerKey = 0x040B,

    ProtocolHandshakeAuthenticationRequest = 0x01C5,
    ProtocolHandshakeAuthenticationResponse = 0x01AF,

    LobbyWorldListRequest = 0x01C0,
    LobbyWorldListResponse = 0x01C1,
    LobbyWorldEntry = 0x01E5,
    LobbyWorldSelectRequest = 0x019D,
    LobbyWorldSelectResponse = 0x019E,

    LobbyCharacterListRequest = 0x01B3,
    LobbyCharacterListResponse = 0x01B4,
    LobbyCharacterEntry = 0x01D7,
    LobbyCharacterUnknown1 = 0x01DD,
    LobbyCharacterUnknown2 = 0x0DBF,
    LobbyCharacterUnknown3 = 0x0E10,
    LobbyCharacterUnknown4 = 0x00DE,
    LobbyCharacterUnknown5 = 0x0080,
    LobbyCharacterUnknown6 = 0x1E17,
    LobbyCharacterCreationCacheRequest = 0x01C2,
    LobbyCharacterCreationCacheResponse = 0x01C3,
    CacheUpdate = 0x0025,
    TemplateCreationData = 0x027E,
    TemplateCreationUnknown1Data = 0x0ECD,
    TemplateCreationSubData = 0x0274,
    TemplateCreationSubUnknown1Data = 0x1C97,
    TemplateCreationSubUnknown2Data = 0x1C9D,
    TemplateCreationSubUnknown3Data = 0x1C9C,
    TemplateCreationSubUnknown4Data = 0x1CD2,
    TemplateCreationSubUnknown5Data = 0x1E17,


    IPCWorldRegisterRequest = 0x010000,
    IPCWorldRegisterResponse = 0x010001,
    IPCWorldPopulationUpdate = 0x010002,
}