# Proyecto 3: ERP to Shopify 🚀

## 📖 Descripción del Proyecto

Este proyecto consiste en un middleware de integración construido con **Azure Functions (.NET 10)**. Su propósito principal es servir como puente para automatizar la creación y sincronización de un catálogo de productos desde un sistema externo (como un ERP) hacia una tienda en **Shopify**.

El gran valor de este desarrollo radica en facilitar la traducción de datos: el sistema recibe un archivo JSON con una estructura simple y amigable, y el backend se encarga de procesar esa información y traducirla automáticamente al formato específico (y más complejo) que requiere la API de Shopify para dar de alta los productos.

---

## ✨ Características Principales

* **Recepción Simplificada:** Interfaz de usuario pura (HTML/CSS/JS) para cargar catálogos de prueba.
* **Traducción Automática:** Mapeo de campos desde el modelo de datos del negocio hacia los requerimientos exactos de Shopify.
* **Arquitectura Escalable:** Uso de .NET Isolated Worker con separación de responsabilidades para mantener el código limpio y fácil de mantener.

---

## ⚙️ Requisitos Previos y Configuración de Shopify

Para que esta integración funcione correctamente y pueda comunicarse con la plataforma, es **estrictamente necesario** contar con una tienda activa y sus respectivas credenciales. Los pasos a seguir son:

1. **Crear una cuenta en Shopify** e inicializar tu tienda.
2. Ir a la configuración de la tienda (Settings > Apps and sales channels) y crear una **Aplicación Personalizada (Custom App)**.
3. Configurar los alcances de la API (API Scopes) otorgando permisos de escritura y lectura para **Productos** e **Inventario**.
4. Instalar la aplicación para generar tu **Admin API Access Token**.

---

## 🔐 Configuración de Credenciales (API Keys)

Por motivos de seguridad, las credenciales de tu tienda **nunca deben subirse a este repositorio**. 

Para ejecutar el proyecto en tu entorno local, debes agregar las variables de entorno en tu archivo de configuración. Al tratarse de una Azure Function, la configuración que en una API tradicional de .NET iría en el `appsettings.json`, aquí se maneja a través del archivo **`local.settings.json`**.

Crea o edita el archivo `local.settings.json` dentro de tu proyecto backend con la siguiente estructura, reemplazando los valores con los de tu tienda:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ShopifyUrl": "[https://tu-tienda.myshopify.com](https://tu-tienda.myshopify.com)",
    "ShopifyAccessToken": "shpat_tu_token_generado_aqui"
  },
  "Host": {
    "CORS": "*",
    "CORSCredentials": false
  }
}
