BEGIN TRANSACTION;
CREATE TABLE "Combate" (
    "IdPartida" TEXT NOT NULL,
    "NumeroRonda" INTEGER NOT NULL,
    "Numero" INTEGER NOT NULL,
    "Elector" TEXT NOT NULL,
    "IdCartaElector" INTEGER NULL,
    "IdCartaRival" INTEGER NULL,
    "Atributo" TEXT NULL,
    "PuntajeElector" INTEGER NULL,
    "PuntajeRival" INTEGER NULL,
    "Ganador" TEXT NULL,
    CONSTRAINT "PK_Combate" PRIMARY KEY ("IdPartida", "NumeroRonda", "Numero"),
    CONSTRAINT "FK_Combate_Ronda_IdPartida_NumeroRonda" FOREIGN KEY ("IdPartida", "NumeroRonda") REFERENCES "Ronda" ("IdPartida", "Numero") ON DELETE CASCADE
);
INSERT INTO "Combate" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',1,1,'J1',6,NULL,'Fuerza',NULL,NULL,NULL);
CREATE TABLE "Operacion" (
    "IdPartida" TEXT NOT NULL,
    "Rol" TEXT NOT NULL,
    "IdOperacion" TEXT NOT NULL,
    "IdSolicitud" TEXT NOT NULL,
    "RevisionEsperada" INTEGER NOT NULL,
    "IdPersonaje" INTEGER NOT NULL,
    "Atributo" TEXT NULL,
    "RevisionConfirmada" INTEGER NOT NULL,
    CONSTRAINT "PK_Operacion" PRIMARY KEY ("IdPartida", "Rol", "IdOperacion"),
    CONSTRAINT "FK_Operacion_Participante_IdPartida_Rol" FOREIGN KEY ("IdPartida", "Rol") REFERENCES "Participante" ("IdPartida", "Rol") ON DELETE CASCADE,
    CONSTRAINT "FK_Operacion_Solicitud_IdSolicitud" FOREIGN KEY ("IdSolicitud") REFERENCES "Solicitud" ("Id") ON DELETE CASCADE
);
INSERT INTO "Operacion" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74','J1','917CE62C-050B-4603-BD58-8530ACF4DAB4','2D9CA837-F037-4C15-B404-C42E95D28A30',1,6,'Fuerza',2);
CREATE TABLE "Participante" (
    "IdPartida" TEXT NOT NULL,
    "Rol" TEXT NOT NULL,
    "Hash" BLOB NOT NULL,
    "Nombre" TEXT NULL,
    "RondasGanadas" INTEGER NOT NULL,
    CONSTRAINT "PK_Participante" PRIMARY KEY ("IdPartida", "Rol"),
    CONSTRAINT "FK_Participante_Partida_IdPartida" FOREIGN KEY ("IdPartida") REFERENCES "Partida" ("Id") ON DELETE CASCADE
);
INSERT INTO "Participante" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74','J1',X'7066E9BCCDBDF369BDC97AB9A135B042677A05EA3D7AB0C770A9AADEB9510B85','Ada',0);
INSERT INTO "Participante" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74','J2',X'A024C1C855024DBFD331007DF8B8AD4A6D4FD20854FF331EBE81BBD8A2178B37','Bruno',0);
CREATE TABLE "Partida" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Partida" PRIMARY KEY,
    "Revision" INTEGER NOT NULL,
    "NumeroRonda" INTEGER NULL,
    "NumeroCombate" INTEGER NULL,
    "IdSolicitud" TEXT NULL,
    "MotivoFin" TEXT NULL,
    "Ganador" TEXT NULL,
    "AbandonoDe" TEXT NULL,
    "Fase" TEXT NOT NULL
);
INSERT INTO "Partida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',2,1,1,'F5F4D127-52F5-481E-909C-AEA44A423341',NULL,NULL,NULL,'esperandoRival');
CREATE TABLE "PersonajeCatalogo" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PersonajeCatalogo" PRIMARY KEY,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "Ascii" TEXT NOT NULL,
    "Fuerza" INTEGER NOT NULL,
    "Velocidad" INTEGER NOT NULL,
    "Resistencia" INTEGER NOT NULL,
    "Inteligencia" INTEGER NOT NULL,
    "Energia" INTEGER NOT NULL,
    "Habilidad" INTEGER NOT NULL,
    "Activo" INTEGER NOT NULL
);
INSERT INTO "PersonajeCatalogo" VALUES(1,'Anterior 0','','*',750,550,500,250,500,450,1);
INSERT INTO "PersonajeCatalogo" VALUES(2,'Anterior 1','','*',750,550,500,250,500,450,1);
INSERT INTO "PersonajeCatalogo" VALUES(3,'Anterior 2','','*',750,550,500,250,500,450,1);
INSERT INTO "PersonajeCatalogo" VALUES(4,'Anterior 3','','*',750,550,500,250,500,450,1);
INSERT INTO "PersonajeCatalogo" VALUES(5,'Anterior 4','','*',750,550,500,250,500,450,1);
INSERT INTO "PersonajeCatalogo" VALUES(6,'Anterior 5','','*',750,550,500,250,500,450,1);
CREATE TABLE "PersonajePartida" (
    "IdPartida" TEXT NOT NULL,
    "IdPersonaje" INTEGER NOT NULL,
    "Nombre" TEXT NOT NULL,
    "Descripcion" TEXT NOT NULL,
    "Ascii" TEXT NOT NULL,
    "Fuerza" INTEGER NOT NULL,
    "Velocidad" INTEGER NOT NULL,
    "Resistencia" INTEGER NOT NULL,
    "Inteligencia" INTEGER NOT NULL,
    "Energia" INTEGER NOT NULL,
    "Habilidad" INTEGER NOT NULL,
    "Ubicacion" TEXT NOT NULL,
    "Orden" INTEGER NOT NULL,
    CONSTRAINT "PK_PersonajePartida" PRIMARY KEY ("IdPartida", "IdPersonaje"),
    CONSTRAINT "FK_PersonajePartida_Partida_IdPartida" FOREIGN KEY ("IdPartida") REFERENCES "Partida" ("Id") ON DELETE CASCADE
);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',1,'Anterior 0','','*',750,550,500,250,500,450,'J2',5);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',2,'Anterior 1','','*',750,550,500,250,500,450,'J2',3);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',3,'Anterior 2','','*',750,550,500,250,500,450,'J1',1);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',4,'Anterior 3','','*',750,550,500,250,500,450,'J2',4);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',5,'Anterior 4','','*',750,550,500,250,500,450,'J1',2);
INSERT INTO "PersonajePartida" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',6,'Anterior 5','','*',750,550,500,250,500,450,'J1',0);
CREATE TABLE "Ronda" (
    "IdPartida" TEXT NOT NULL,
    "Numero" INTEGER NOT NULL,
    "Mano" TEXT NOT NULL,
    "VictoriasJ1" INTEGER NOT NULL,
    "VictoriasJ2" INTEGER NOT NULL,
    "Ganador" TEXT NULL,
    CONSTRAINT "PK_Ronda" PRIMARY KEY ("IdPartida", "Numero"),
    CONSTRAINT "FK_Ronda_Partida_IdPartida" FOREIGN KEY ("IdPartida") REFERENCES "Partida" ("Id") ON DELETE CASCADE
);
INSERT INTO "Ronda" VALUES('F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',1,'J1',0,0,NULL);
CREATE TABLE "Solicitud" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Solicitud" PRIMARY KEY,
    "IdPartida" TEXT NOT NULL,
    "NumeroRonda" INTEGER NOT NULL,
    "NumeroCombate" INTEGER NOT NULL,
    "Rol" TEXT NOT NULL,
    "Tipo" TEXT NOT NULL,
    "Consumida" INTEGER NOT NULL,
    CONSTRAINT "FK_Solicitud_Combate_IdPartida_NumeroRonda_NumeroCombate" FOREIGN KEY ("IdPartida", "NumeroRonda", "NumeroCombate") REFERENCES "Combate" ("IdPartida", "NumeroRonda", "Numero") ON DELETE CASCADE
);
INSERT INTO "Solicitud" VALUES('2D9CA837-F037-4C15-B404-C42E95D28A30','F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',1,1,'J1','cartaYAtributo',1);
INSERT INTO "Solicitud" VALUES('F5F4D127-52F5-481E-909C-AEA44A423341','F3A7A2B0-54AF-43AE-ABFF-FD768B65AA74',1,1,'J2','carta',0);
CREATE INDEX "IX_Operacion_IdSolicitud" ON "Operacion" ("IdSolicitud");
CREATE INDEX "IX_Solicitud_IdPartida" ON "Solicitud" ("IdPartida");
CREATE INDEX "IX_Solicitud_IdPartida_NumeroRonda_NumeroCombate" ON "Solicitud" ("IdPartida", "NumeroRonda", "NumeroCombate");
COMMIT;
