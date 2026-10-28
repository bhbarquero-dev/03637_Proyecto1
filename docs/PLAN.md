# Plan de trabajo — Proyecto No. 1 (03637 Gestión de Sitios Web, III C 2026)

Plataforma web de la Asociación de Emprendedoras — incremento funcional "Solicitud de compra sin cuenta".

> Base: enunciado del Proyecto No. 1 y propuesta de la Tarea 1 (diagnóstico, RF/RNF, trazabilidad).
> Todos los datos del proyecto son **sintéticos** (S-05 de la Tarea 1 y condición de entrega del enunciado).

---

## 1. Qué pide el enunciado (resumen operativo)

| Exigencia | Cómo se cumple en este plan |
|---|---|
| Incremento acotado que resuelva **un flujo de usuario de principio a fin** | Flujo: catálogo → solicitud de compra → número de seguimiento → gestión de estado por el personal (§3) |
| Interfaz + validación de datos + persistencia + evidencia de pruebas | Razor Pages, validación cliente/servidor, SQLite con EF Core, xUnit + Playwright (§4, §9) |
| Repo privado, sin credenciales ni datos sensibles | Repo ya es **PRIVATE**; secretos solo en `dotnet user-secrets` / variables de entorno; `.gitignore` para `*.db` |
| Ramas, commits significativos, revisión de cambios, etiquetas/versiones | GitHub Flow + PR con autorrevisión + tags `v0.x` / `v1.0.0` + release notes (§7) |
| Trazabilidad requisito → historia → criterio → código → prueba | Issues de GitHub por historia, PR que cierra el issue, ID de prueba en el nombre del test, matriz en `docs/trazabilidad.md` (§6) |
| Accesibilidad, seguridad básica, privacidad, lenguaje inclusivo, eficiencia | RNF-01, RNF-03, RNF-05, Ley 8968, guía de lenguaje inclusivo (§10) |
| Tablero, bitácora, defensa técnica | GitHub Projects + `docs/bitacora.md` + video (§5, §11) |

**Fases del enunciado → secciones de este plan:** Preparación (§5) · Hipótesis (§8) · Implementación controlada (§3, §4, §6) · Gestión de versiones (§7) · Gestión de contenidos (§10.1) · Pruebas (§9) · Revisión y retrospectiva (§11).

---

## 2. Decisión de alcance frente a la Tarea 1

La Tarea 1 recomendó la **alternativa B** (plataforma por suscripción) y condicionó la decisión a una **prueba de concepto** de RF-06, RF-07 y RNF-03. El Proyecto 1 exige código propio, ramas y pruebas, por lo que el incremento se plantea así:

- **Enfoque:** incremento desarrollado a medida que funciona como **prueba de concepto del flujo crítico** (solicitudes, N-02), para validar requerimientos, accesibilidad y rendimiento antes de comprometer la plataforma definitiva.
- **Justificación en el informe:** el flujo de solicitudes es el de mayor peso en el ROI (crecimiento de solicitudes = variable más sensible, Tabla 8 de la Tarea 1) y el que más impacta CE-01, CE-03 y CE-07.
- **Cambio respecto de la línea base:** se registra en la bitácora como decisión D-01 (cambio de alternativa para el incremento, con su razón).

> ⚠️ Decisión a confirmar: si prefieres que el incremento sea el de **historias con consentimiento (RF-07)**, el plan cambia en §3; el resto se mantiene igual.

---

## 3. Alcance del incremento

### Dentro (MVP del incremento)

| ID | Historia de usuario | RF/RNF origen | Prioridad |
|---|---|---|---|
| HU-01 | Como **persona compradora**, quiero ver el catálogo con foto, precio y disponibilidad para decidir qué solicitar. | RF-01 (lectura), RNF-01, RNF-02 | Must |
| HU-02 | Como **persona compradora**, quiero registrar una solicitud de compra **sin crear cuenta**, dando mi consentimiento, para recibir un número de seguimiento. | RF-03, Ley 8968 | Must |
| HU-03 | Como **persona compradora**, quiero consultar el estado de mi solicitud con mi número y correo. | RF-04 (lectura), N-02 | Must |
| HU-04 | Como **personal administrativo**, quiero iniciar sesión y ver las solicitudes, para gestionarlas en un solo lugar. | RF-09 (rol administrativo), RNF-05 | Must |
| HU-05 | Como **personal administrativo**, quiero cambiar el estado de una solicitud y que quede registrado quién, cuándo y de qué a qué. | RF-04 | Must |
| HU-06 | Como **persona usuaria**, quiero cambiar la interfaz entre español e inglés sin perder lo que estoy haciendo. | RF-08 | Should |
| HU-07 | Como **persona compradora**, quiero solicitar la eliminación de mis datos de una solicitud. | RF-11 (registro de la solicitud de eliminación) | Could |

### Criterios de aceptación (borrador, se refinan en el Issue)

- **HU-01:** el catálogo pesa ≤ 1 MB y carga en ≤ 5 s con 3G simulada; un producto **no disponible** muestra "Agotado" y no tiene botón de solicitar; funciona a 360 px de ancho.
- **HU-02:** campos obligatorios (nombre, correo o teléfono, cantidad 1–20, producto disponible); no se envía sin marcar el consentimiento; al enviar se genera número `SOL-AAAA-NNNN` en estado **Recibida**; mensajes de error asociados al campo (WCAG 3.3.1/3.3.3); protección CSRF.
- **HU-03:** con número + correo correctos se ve estado e historial; con datos incorrectos se muestra un mensaje genérico (no revela si el número existe).
- **HU-04:** sin sesión, `/Admin/*` redirige a inicio de sesión; contraseñas con hash (ASP.NET Core Identity); bloqueo tras 5 intentos fallidos.
- **HU-05:** transiciones válidas: Recibida → Confirmada → En entrega → Entregada; cualquiera (excepto Entregada) → Cancelada; transición inválida rechazada en el servidor; cada cambio guarda fecha, hora, usuario, estado anterior y nuevo.
- **HU-06:** todos los textos de las páginas del flujo existen en `es` y `en`; el idioma persiste por cookie; `lang` del `<html>` cambia.
- **HU-07:** se registra la solicitud de eliminación; el personal la atiende y los datos personales se anonimizan (no se borra el registro de la venta).

### Fuera del incremento (se declaran como límites)

Pagos en línea, avisos por correo/SMS (RF-05: se simula con registro en log), entregas (RF-06), historias (RF-07), reportes (RF-10), panel de emprendedoras para editar productos (RF-01 escritura: el catálogo se carga con datos sembrados), despliegue en producción.

---

## 4. Solución técnica

| Capa | Elección | Razón |
|---|---|---|
| Plataforma | **ASP.NET Core 10 — Razor Pages** (SDK 10.0.403 instalado) | Renderizado en servidor = páginas livianas para 3G y gama baja (RNF-01); el `.gitignore` del repo ya es de Visual Studio |
| Datos | **EF Core + SQLite** | Persistencia real sin servidor ni costo; archivo `.db` fuera del repo; migraciones versionadas |
| Autenticación | **ASP.NET Core Identity** (solo rol Administración) | Hash de contraseñas, bloqueo, cookies seguras (RNF-05) |
| i18n | `IStringLocalizer` + archivos `.resx` es/en | Nativo, sin dependencias |
| Estilos | CSS propio mínimo, sin framework pesado | Presupuesto de peso ≤ 1 MB; contraste AA |
| Imágenes | WebP comprimidas, `loading="lazy"`, `width/height` fijos | RNF-01 y eficiencia de recursos |
| Pruebas | xUnit (unitarias), `WebApplicationFactory` (integración), **Playwright for .NET** (funcionales/E2E), **Deque.AxeCore.Playwright** (accesibilidad) | Todo en .NET, ejecutable con `dotnet test` |
| CI | GitHub Actions: build + test en cada PR | Evidencia automática de revisión de cambios |

### Estructura propuesta del repositorio

```
03637_Proyecto1/
├─ src/Emprendedoras.Web/          # Razor Pages, Data/, Domain/, Resources/ (es, en), wwwroot/
├─ tests/Emprendedoras.UnitTests/
├─ tests/Emprendedoras.IntegrationTests/
├─ tests/Emprendedoras.E2ETests/   # Playwright + axe
├─ docs/
│  ├─ PLAN.md                      # este documento
│  ├─ definition-of-ready-done.md
│  ├─ bitacora.md                  # decisiones, impedimentos, cambios (fechado)
│  ├─ trazabilidad.md              # RF → HU → criterio → PR/commit → prueba → resultado
│  ├─ hipotesis.md
│  ├─ gestion-contenidos.md        # matriz + procedimiento
│  ├─ pruebas/plan-pruebas.md, casos.md, defectos.md, reporte.md, evidencias/
│  └─ retrospectiva.md
├─ scripts/backup-db.ps1           # respaldo del archivo SQLite
├─ .github/workflows/ci.yml, pull_request_template.md, ISSUE_TEMPLATE/
├─ CHANGELOG.md
└─ README.md                       # cómo abrir, ejecutar y probar (rúbrica: organización de archivos)
```

---

## 5. Preparación de la ejecución

**Objetivo de la iteración:** "Una persona compradora puede registrar y consultar una solicitud de compra, y el personal administrativo puede gestionar su estado, de forma accesible y con datos protegidos."

**Definition of Ready (una HU entra a desarrollo si):** tiene ID y vínculo a RF/RNF; criterios de aceptación verificables; casos de prueba identificados; tamaño ≤ 1 día; sin dependencias bloqueantes.

**Definition of Done (una HU se cierra si):** código en `main` vía PR revisado; CI en verde; pruebas de sus criterios pasando; sin incumplimientos axe A/AA en sus páginas; textos en es/en (si aplica); trazabilidad y bitácora actualizadas; sin secretos en el diff.

**Tablero (GitHub Projects):** columnas *Backlog → Ready → En progreso → En revisión → Done* + campos *HU*, *RF*, *Fecha inicio/fin*, *Estimado/real (h)*. Un Issue por HU y por tarea técnica (configuración, CI, documentación). El tablero exportado alimenta el **registro de seguimiento** del informe.

### Cronograma propuesto (≈ 4 semanas)

> Supuesto: inicio 28-oct-2026. **Ajustar a la fecha de entrega real del curso.**

| Semana | Fechas | Trabajo | Release |
|---|---|---|---|
| S0 | 28–31 oct | DoR/DoD, tablero, Issues, hipótesis, esqueleto de solución, CI, plantilla de PR | `v0.1.0` (esqueleto) |
| S1 | 1–8 nov | HU-01, HU-02 (+ unitarias e integración) | `v0.2.0` |
| S2 | 9–15 nov | HU-03, HU-04, HU-05, HU-06; matriz de contenidos; script de respaldo | `v0.3.0` |
| S3 | 16–22 nov | Pruebas funcionales, accesibilidad, rendimiento 3G, usabilidad; defectos y re-pruebas; HU-07 si hay holgura | `v1.0.0` + release notes |
| S4 | 23–29 nov | Informe (12–16 pp.), tabla de comprobación, retrospectiva, video 6–8 min, .zip, autoevaluación con rúbrica | — |

---

## 6. Implementación controlada y trazabilidad

- Cada HU = Issue `HU-0X`; cada tarea técnica = Issue `T-0X`.
- Rama por Issue: `feature/HU-02-solicitud-compra`, `fix/DEF-03-...`, `docs/...`.
- Commits en formato *Conventional Commits* con referencia: `feat(solicitud): valida consentimiento obligatorio (#12)`.
- PR con plantilla: HU/RF, criterios cubiertos, pruebas agregadas, capturas, checklist DoD. **Autorrevisión** del diff antes de fusionar (comentarios en el PR como evidencia).
- Las pruebas se nombran con el caso: `CP_HU02_03_RechazaSinConsentimiento`.
- `docs/trazabilidad.md`: RF → HU → criterio → PR/commit → caso de prueba → resultado.
- `docs/bitacora.md` (entrada fechada): decisiones (D-xx), impedimentos (I-xx), cambios a la línea base (C-xx), revisiones.

---

## 7. Gestión de versiones

- **Modelo:** GitHub Flow (`main` siempre estable + ramas cortas). No se hace push directo a `main` (en repos privados del plan gratuito la protección de rama no siempre está disponible: se documenta como regla de trabajo y el historial lo demuestra).
- **Fusión:** *squash* o *merge commit* (elegir uno y declararlo en el informe).
- **Conflictos:** se documenta al menos **un conflicto real resuelto** (p. ej. dos ramas que modifican el mismo `.resx`), con captura y explicación.
- **Versionado:** SemVer con tags anotados (`git tag -a v1.0.0`) y **GitHub Releases** con notas de liberación; `CHANGELOG.md` (formato *Keep a Changelog*).

---

## 8. Hipótesis y medición

**Hipótesis del incremento (H-01):** Si la persona compradora puede registrar una solicitud sin crear cuenta, en un formulario corto, accesible y liviano, entonces al menos **80%** de las personas participantes completará una solicitud **sin ayuda en ≤ 3 minutos** (RNF-04, CE-07), y el registro centralizado eliminará los duplicados y las solicitudes sin estado (N-02).

| Métrica | Meta | Instrumento |
|---|---|---|
| M-01 Tasa de finalización sin ayuda | ≥ 80% | Prueba de usabilidad con 5 participantes y guion de tarea (datos sintéticos) |
| M-02 Tiempo para completar la solicitud | ≤ 3 min (mediana) | Cronómetro / registro de la prueba |
| M-03 Peso de la página de catálogo | ≤ 1 MB | Playwright (suma de transferencias) |
| M-04 Tiempo de carga con 3G simulada | ≤ 5 s | Playwright + emulación de red (CDP) |
| M-05 Incumplimientos WCAG 2.2 A/AA | 0 | axe-core automatizado + revisión manual teclado/lector de pantalla (NVDA) |
| M-06 Solicitudes con estado e historial | 100% | Prueba de integración + consulta a BD |
| M-07 Defectos críticos abiertos al release | 0 | `docs/pruebas/defectos.md` |

**Criterio de decisión:** se acepta H-01 si M-01 y M-02 se cumplen; si no, se registran causas y ajustes (y se re-prueba si el tiempo lo permite).

---

## 9. Pruebas y evaluación

| Nivel | Qué cubre | Herramienta |
|---|---|---|
| Unitarias | Reglas de transición de estados, generación del número de solicitud, validaciones | xUnit |
| Integración | Páginas + BD SQLite en memoria; CSRF; acceso a `/Admin` sin sesión; historial de estados | `WebApplicationFactory` |
| Funcionales / E2E | Flujos HU-01 a HU-06 de punta a punta | Playwright |
| No funcionales | Peso y carga 3G (RNF-01), 360 px (RNF-02), accesibilidad (RNF-03), cabeceras de seguridad y acceso sin permisos (RNF-05) | Playwright, axe, revisión manual |
| Aceptación | Criterios de cada HU + prueba de usabilidad (H-01) | Lista de verificación |

Registro por caso: ID, HU/RF, entorno (SO, navegador, versión, commit/tag), datos de prueba, resultado esperado, resultado real, estado, defecto vinculado, re-prueba. Esta tabla alimenta la **tabla de comprobación** exigida en el informe.

---

## 10. Ejes transversales

### 10.1 Gestión de contenidos (matriz y procedimiento — `docs/gestion-contenidos.md`)

| Tipo de contenido | Responsable | Permisos | Ciclo (crear → revisar → publicar → actualizar → archivar) | Respaldo |
|---|---|---|---|---|
| Producto (texto, precio, disponibilidad) | Emprendedora (dueña) / Administración | Crear-editar: Administración en el incremento | Borrador → revisión → publicado → oculto (archivado) | BD diaria |
| Fotografía de producto | Emprendedora, con autorización (Ley 6683) | Subir: Administración | Optimizar WebP → publicar → reemplazar | Carpeta `wwwroot/img` versionada (solo imágenes sintéticas/libres) |
| Solicitud de compra (dato personal) | Personal administrativo | Ver/editar estado: Administración; la compradora solo la suya | Recibida → … → Entregada/Cancelada → anonimización según política | BD diaria, retención 30 días |
| Textos de interfaz es/en | Administración (valida traducción, S-03) | Cambio vía PR | PR → revisión → release | Git |
| Política de privacidad y consentimiento | Dirección | Cambio vía PR | Versión fechada | Git |

Procedimiento de respaldo: `scripts/backup-db.ps1` copia el archivo SQLite con fecha y conserva 30 copias; se prueba una restauración (RNF-07).

### 10.2 Accesibilidad, seguridad, privacidad, inclusión y sostenibilidad

- **Accesibilidad:** HTML semántico, etiquetas en todos los campos, foco visible, contraste AA, enlace "saltar al contenido", errores anunciados, objetivos táctiles ≥ 24 px (WCAG 2.2 2.5.8).
- **Seguridad básica:** HTTPS/HSTS, antiforgery, cabeceras (CSP, X-Content-Type-Options), consultas parametrizadas (EF Core), bloqueo de cuenta, mínimo privilegio, secretos fuera del repo.
- **Privacidad (Ley 8968):** minimización de datos (solo lo necesario para la solicitud), consentimiento explícito y separado, aviso de privacidad, derecho de eliminación (HU-07), datos sintéticos.
- **Lenguaje inclusivo:** guía breve en `docs/` (p. ej. "persona compradora", "personal administrativo") aplicada a la interfaz y al informe.
- **Diversidad cultural:** es/en; lenguas de las comunidades como mejora futura (exclusión de la Tarea 1).
- **Eficiencia/sostenibilidad:** presupuesto de peso por página, imágenes optimizadas, sin JavaScript innecesario, SQLite sin servidor dedicado.

---

## 11. Revisión, retrospectiva y entregables

**Revisión final:** recorrer la rúbrica criterio por criterio, verificar coherencia RF ↔ HU ↔ código ↔ pruebas ↔ evidencias, y documentar los ajustes previos a la entrega (lo pide el enunciado).

**Retrospectiva personal:** qué funcionó, qué no, métricas de avance (estimado vs. real por HU, velocidad) y de calidad (defectos encontrados/corregidos, cobertura de criterios), límites y acciones de mejora.

### Lista de entregables

- [ ] **Informe técnico** PDF, 12–16 pp. (sin portada, índice, referencias ni anexos), Arial 11, interlineado 1,15, márgenes 2,54 cm, numeración y títulos jerarquizados.
- [ ] **Registro de seguimiento** dentro del informe (tareas, fechas, estados, dificultades, cambios).
- [ ] **Tabla de comprobación** (aspecto revisado, resultado, problema, ajuste).
- [ ] **Archivos del proyecto** en `.zip` (código sin `bin/obj/*.db`, documentos, evidencias) + enlace/acceso al repo privado según orientaciones del curso.
- [ ] **Video** individual de 6–8 min (producto, proceso, resultados).
- [ ] **Referencias APA**: antología + **≥ 5 fuentes** técnicas verificables (candidatas: WCAG 2.2; ISO/IEC/IEEE 29148; OWASP Top 10 / ASVS; documentación de ASP.NET Core; Conventional Commits / SemVer; ISO/IEC 25010; Ley 8968).
- [ ] Autoevaluación con la rúbrica y ajustes documentados.

### Rúbrica → evidencia principal

| Criterio | Evidencia |
|---|---|
| Planificación y seguimiento individual | Tablero, bitácora, registro de seguimiento |
| Producto desarrollado y exactitud técnica | Incremento funcionando, release `v1.0.0`, video |
| Organización de archivos y revisión | Estructura del repo, README con instrucciones, .zip |
| Gestión de contenidos y ejes transversales | §10, matriz de contenidos, resultados axe |
| Comprobación de requerimientos y resultados | Plan y casos de prueba, defectos, re-pruebas, tabla de comprobación |
| Rigor científico y análisis crítico | Hipótesis + métricas + decisión aceptada/refutada, limitaciones |
| Informe y video | PDF formateado + video |

---

## 12. Riesgos del proyecto académico

| Riesgo | Respuesta |
|---|---|
| Alcance crece más allá del incremento | Solo HU Must/Should; HU-07 únicamente con holgura; cambios registrados en bitácora |
| Poco tiempo para pruebas de usabilidad | Agendar 5 sesiones cortas en S3 desde S0 |
| Secretos o datos reales en el repo | `.gitignore`, `user-secrets`, revisión del diff en cada PR, solo datos sintéticos |
| Playwright/axe dan problemas en Windows | Ejecutar en CI (Ubuntu) como respaldo |
| Integridad académica y uso de IA | Revisar la política del curso y declarar el uso de herramientas de IA en el informe si corresponde |

## 13. Pendientes por confirmar

1. Fecha de entrega exacta (para ajustar §5).
2. Flujo del incremento: solicitudes (propuesto) vs. historias con consentimiento.
3. Mecanismo de acceso al repo privado para el profesor (colaborador o .zip solamente).
