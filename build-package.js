const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Ler o arquivo package.json para obter o nome e a versão dinâmica do plugin
const packageJson = JSON.parse(fs.readFileSync(path.join(__dirname, 'package.json'), 'utf8'));
const PLUGIN_NAME = 'com.alazter.audio_controller.sdPlugin'; // Nome do diretório do plugin
const VERSION = 'v' + packageJson.version; // Ex: v0.1.0

const OUTPUT_FILE_NAME = 'com.alazter.audio_controller.sdPlugin';
const TEMP_DIR = path.join(__dirname, 'temp_build');
const TARGET_DIR = path.join(TEMP_DIR, PLUGIN_NAME);
const ZIP_PATH = path.join(__dirname, 'com.alazter.audio_controller.zip');
const PACKAGE_PATH = path.join(__dirname, OUTPUT_FILE_NAME);

// Localizar a pasta GitHub subindo os níveis necessários de forma segura
let parentDir = path.join(__dirname, '..');
if (path.basename(parentDir).toLowerCase() === 'projetos') {
    parentDir = path.join(parentDir, '..'); // Sobe mais um nível se estiver dentro de 'Projetos'
}
const RELEASES_DIR = path.join(parentDir, 'Releases', PLUGIN_NAME, VERSION);
const FINAL_RELEASE_PATH = path.join(RELEASES_DIR, OUTPUT_FILE_NAME);

console.log(`🚀 Iniciando empacotamento do plugin (${VERSION})...`);

// 1. Limpar diretórios temporários e empacotamento anterior
if (fs.existsSync(TEMP_DIR)) {
    fs.rmSync(TEMP_DIR, { recursive: true, force: true });
}
if (fs.existsSync(ZIP_PATH)) {
    fs.rmSync(ZIP_PATH, { force: true });
}
if (fs.existsSync(PACKAGE_PATH)) {
    fs.rmSync(PACKAGE_PATH, { force: true });
}

// Criar estrutura de pastas
fs.mkdirSync(TARGET_DIR, { recursive: true });

// Lista de arquivos/pastas permitidos para distribuição
const ALLOWED_ENTRIES = [
    'manifest.json',
    'server.js',
    'VolumeControl.exe',
    'run.bat',
    'index.html',
    'en.json',
    'es.json',
    'ja.json',
    'zh_CN.json',
    'package.json',
    'package-lock.json',
    'node_modules',
    'propertyInspector',
    'static',
    'images',
    'fonts',
    'assets'
];

console.log("📂 Copiando arquivos necessários...");

for (const entry of ALLOWED_ENTRIES) {
    const srcPath = path.join(__dirname, entry);
    const destPath = path.join(TARGET_DIR, entry);
    
    if (fs.existsSync(srcPath)) {
        fs.cpSync(srcPath, destPath, { 
            recursive: true,
            filter: (src) => {
                const basename = path.basename(src);
                if (basename.endsWith('.log') || basename.endsWith('.bak') || basename.endsWith('.backup') || basename.endsWith('.sdPlugin') || basename.endsWith('.SDPlugin')) {
                    return false;
                }
                return true;
            }
        });
    }
}

console.log("🤐 Compactando em arquivo temporário ZIP...");

try {
    // 1. Compactar como .zip primeiro (exigido pelo PowerShell Compress-Archive)
    execSync(`powershell -Command "Compress-Archive -Path '${TARGET_DIR}\\*' -DestinationPath '${ZIP_PATH}' -Force"`);
    
    // 2. Renomear o .zip para o arquivo final .SDPlugin
    fs.renameSync(ZIP_PATH, PACKAGE_PATH);
    console.log(`✨ Arquivo .SDPlugin gerado com sucesso: ${PACKAGE_PATH}`);
    
    // ---- ORGANIZAR E SALVAR NA PASTA DE RELEASES ----
    console.log(`🚚 Copiando instalador para o diretório de Releases organizado...`);
    fs.mkdirSync(RELEASES_DIR, { recursive: true });
    fs.copyFileSync(PACKAGE_PATH, FINAL_RELEASE_PATH);
    console.log(`✅ Lançamento local organizado e copiado com sucesso para: ${FINAL_RELEASE_PATH}`);
    
    // ---- COPIAR PARA A PASTA DE ASSETS DO REPOSITÓRIO ----
    console.log(`📂 Copiando para a pasta assets local do repositório...`);
    const LOCAL_ASSETS_PATH = path.join(__dirname, 'assets', OUTPUT_FILE_NAME);
    fs.copyFileSync(PACKAGE_PATH, LOCAL_ASSETS_PATH);
    console.log(`✅ Copiado com sucesso para assets: ${LOCAL_ASSETS_PATH}`);
    
} catch (error) {
    console.error("❌ Erro ao compactar/organizar arquivos:", error);
} finally {
    // Limpar pasta temporária
    console.log("🧹 Limpando arquivos temporários...");
    if (fs.existsSync(TEMP_DIR)) {
        fs.rmSync(TEMP_DIR, { recursive: true, force: true });
    }
}

console.log("🎉 Concluído!");
