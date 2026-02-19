# 1. Inicializar el repositorio local
git init

# 2. Añadir todos los archivos al "paquete" (incluyendo el Dockerfile que creamos antes)
git add .

# 3. Crear el primer commit
git commit -m "Primer despliegue para Dokploy"

# 4. Vincular con tu GitHub (Copia la línea exacta que te sale en la web de GitHub, será algo así)
git remote add origin https://github.com/TU_USUARIO/SistemaClinicaPostgres.git

# 5. Cambiar a la rama principal
git branch -M main

# 6. Subir el código
git push -u origin main