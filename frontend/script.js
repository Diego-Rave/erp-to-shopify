document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('jsonFileInput');
    const syncButton = document.getElementById('syncButton');
    const statusIndicator = document.getElementById('statusIndicator');
    const statusMessage = document.getElementById('statusMessage');

    // Habilitar el botón solo si hay un archivo seleccionado
    fileInput.addEventListener('change', () => {
        syncButton.disabled = fileInput.files.length === 0;
    });

    syncButton.addEventListener('click', async () => {
        const file = fileInput.files[0];
        if (!file) return;

        setUIState('loading', 'Sincronizando... por favor espera.');

        const reader = new FileReader();
        
        reader.onload = async (e) => {
            try {
                // Verificamos que sea un JSON válido antes de enviarlo
                const jsonContent = JSON.parse(e.target.result);

                // Reemplaza localhost por la URL real si en algún momento lo subes a otro lado
                const response = await fetch('http://localhost:7071/api/productos/sincronizar', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(jsonContent)
                });

                if (response.ok) {
                    setUIState('success', '¡Catálogo sincronizado con éxito!');
                } else {
                    throw new Error(`Error del servidor: ${response.status}`);
                }
            } catch (error) {
                console.error('Detalle del error:', error);
                setUIState('error', `Hubo un error: ${error.message}`);
            }
        };

        reader.onerror = () => {
            setUIState('error', 'Error al leer el archivo en el navegador.');
        };

        // Leer el archivo como texto
        reader.readAsText(file);
    });

    function setUIState(state, message) {
        statusIndicator.className = `status ${state}`;
        statusMessage.textContent = message;
        
        // Deshabilitar input y botón mientras carga
        const isLoading = state === 'loading';
        fileInput.disabled = isLoading;
        syncButton.disabled = isLoading;
    }
});