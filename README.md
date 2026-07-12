# Shooter3DDoom

Shooter en primera persona estilo **Doom clásico**, hecho en **Unity 6 (URP)**. Parcial práctico del curso: partiendo de un proyecto base con movimiento y disparo, se amplió con IA de enemigos, navegación con NavMesh, interfaz, sonido y condiciones de victoria/derrota.

## Características

- 🔫 **Dos armas intercambiables** (tecla Q): escopeta semiautomática y metralleta automática (dispara manteniendo el botón, mayor cadencia y cargador de 30).
- 🔄 **Munición y recarga**: balas limitadas por arma, recarga con R tras una espera (corrutina), munición visible en el HUD.
- 👾 **Enemigos con IA**: sprites estilo Doom (billboard que siempre encara la cámara) que persiguen al jugador con **NavMeshAgent** sobre un **NavMesh horneado** del laberinto, y disparan por raycast solo con línea de visión.
- 🎯 **Contador de enemigos + victoria doble**: la meta (pilar verde) solo termina el nivel si eliminaste a todos los enemigos.
- 💀 **Game Over con reinicio**: al morir aparece un menú con botón Reintentar (el juego se congela y se libera el cursor).
- 🩸 **Feedback de daño**: parpadeo rojo en pantalla y sonido al recibir impactos.
- ⛑️ **Botiquines**: curan con tope en la vida máxima y desaparecen con sonido.

## Controles

| Tecla | Acción |
|-------|--------|
| W A S D | Moverse |
| Ratón | Mirar |
| Clic izquierdo | Disparar (mantener con la metralleta) |
| R | Recargar |
| Q (o 1 / 2) | Cambiar de arma |

## Cómo abrirlo

1. Unity **6000.5.3f1** (o superior de Unity 6) con el paquete AI Navigation (incluido en el manifiesto).
2. Abrir esta carpeta como proyecto desde Unity Hub.
3. Abrir la escena `Assets/Scenes/SampleScene` y pulsar Play.

> El menú **Herramientas ▸ Configurar Parcial** regenera toda la configuración de la escena (enemigos, HUD, NavMesh, meta y botiquines) desde cero.

## Estructura de scripts (`Assets/Scripts`)

| Script | Responsabilidad |
|--------|-----------------|
| `PrimeraPersona` | Movimiento y cámara del jugador |
| `Disparar` | Armas, munición, recarga y cambio de arma |
| `Vida` | Vida de jugador y enemigos, curación con tope |
| `EnemigoIA` | Persecución con NavMeshAgent y ataque con raycast |
| `EnemigoBillboard` | El sprite del enemigo siempre mira a la cámara |
| `GestorJuego` | Estado de la partida, HUD, victoria/derrota y reinicio |
| `Meta` | Zona de meta con condición doble de victoria |
| `Botiquin` | Recogible de curación |

## Créditos

- Proyecto base (nivel, sprites de escopeta/enemigo/botiquín, sonidos y texturas): material del curso.
- Ampliaciones del parcial, sprite de la metralleta y sonido del botiquín: elaboración propia.
