# Guía de Contribución

## 👥 Autores del Proyecto

- **Johan Eduardo Cala Torra** - [JohanCalaT](https://github.com/JohanCalaT) - johan.eduardo.cala2002@gmail.com
- **jct576** - [jct576](https://github.com/jct576) - jct576@inlumine.ual.es

---

## 📝 Reglas para Commits

### ⚠️ IMPORTANTE: Todos los commits deben incluir co-autoría

Para que los commits aparezcan en ambos perfiles de GitHub, **SIEMPRE** debes incluir la línea de co-autoría en tus commits.

### 🔧 Cómo hacer commits correctamente

#### **Opción 1: Commit con mensaje multilínea (Recomendado)**

```bash
git add .
git commit -m "Título del commit" -m "" -m "Co-authored-by: jct576 <jct576@inlumine.ual.es>"
git push
```

#### **Opción 2: Commit usando el editor**

```bash
git add .
git commit
```

En el editor que se abre, escribe:

```
Título del commit

Descripción detallada del commit (opcional)

Co-authored-by: jct576 <jct576@inlumine.ual.es>
```

Guarda y cierra el editor.

```bash
git push
```

#### **Opción 3: Usando archivo de mensaje**

Crea un archivo temporal con el mensaje:

```bash
echo "Título del commit

Descripción del commit

Co-authored-by: jct576 <jct576@inlumine.ual.es>" > commit_msg.txt

git add .
git commit -F commit_msg.txt
git push
rm commit_msg.txt
```

---

## ✅ Verificar que el commit tiene co-autoría

Después de hacer commit, verifica que incluye el co-autor:

```bash
git log --pretty=fuller -1
```

Deberías ver algo como:

```
Author:     Johan Cala <johan.eduardo.cala2002@gmail.com>
...
Co-authored-by: jct576 <jct576@inlumine.ual.es>
```

---

## 📋 Convenciones de Commits

### Formato del mensaje:

```
<tipo>: <descripción breve>

<descripción detallada (opcional)>

Co-authored-by: jct576 <jct576@inlumine.ual.es>
```

### Tipos de commits:

- `feat`: Nueva funcionalidad
- `fix`: Corrección de errores
- `docs`: Cambios en documentación
- `style`: Cambios de formato (no afectan la lógica)
- `refactor`: Refactorización de código
- `test`: Añadir o modificar tests
- `chore`: Tareas de mantenimiento

### Ejemplos:

```bash
git commit -m "feat: Agregar sistema de autenticación de usuarios" -m "" -m "Co-authored-by: jct576 <jct576@inlumine.ual.es>"

git commit -m "fix: Corregir validación de formulario de citas" -m "" -m "Co-authored-by: jct576 <jct576@inlumine.ual.es>"

git commit -m "docs: Actualizar README con instrucciones de instalación" -m "" -m "Co-authored-by: jct576 <jct576@inlumine.ual.es>"
```

---

## 🚫 Archivos Ignorados

El archivo `.gitignore` está configurado para ignorar:

- Archivos de compilación de .NET (`bin/`, `obj/`, etc.)
- Archivos de configuración de IDEs (`.vs/`, `.vscode/`, `.idea/`)
- Paquetes NuGet
- **Archivos de Augment AI** (`.augment/`, `.augmentignore`, `augment.config.json`)
- Variables de entorno (`.env`)
- Bases de datos locales
- Logs

---

## 🔄 Flujo de Trabajo

1. **Hacer cambios en el código**
2. **Verificar cambios:** `git status`
3. **Agregar archivos:** `git add .`
4. **Commit con co-autoría** (usar una de las opciones anteriores)
5. **Push al repositorio:** `git push`

---

## 📞 Contacto

Si tienes dudas sobre cómo contribuir, contacta a:
- Johan Eduardo Cala Torra - johan.eduardo.cala2002@gmail.com

