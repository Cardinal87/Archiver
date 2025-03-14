const otherFiles = [];
const images = [];
const textFiles = [];
const STORAGE_IMAGES = "storaged_images";
const STORAGE_TEXT_FILES = "storaged_text_files";
const STORAGE_OTHER = "storaged_other";
let ind = "";



const allowedImages = new Set(["image/jpeg", "image/jpg", "image/png"]);
const allowedTextFiles = new Set(['txt', 'csv', 'json', 'xml', 'log', 'config', 'ini', 'html', 'htm', 'css', 'py', 'java', 'cs']);


document.addEventListener('DOMContentLoaded', async () => {
    const dropZone = document.getElementById('dragZone');
    const fileZone = document.getElementById('files');
    await loadFilesFromStorage();

    if (otherFiles.length === 0 && images.length === 0 && textFiles.length === 0) {
        dropZone.style.display = 'inline-block';
    }
    else {
        fileZone.style.display = 'inline-block';
    }

    
    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-zone');
        dropZone.classList.add('drag-zone-active');
    });
    dropZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-zone');
        dropZone.classList.remove('drag-zone-active');
        if (otherFiles.length === 0 && images.length === 0 && textFiles.length === 0) {
            dropZone.style.display = 'inline-block';
            fileZone.style.display = 'none';
        }
        else {
            dropZone.style.display = 'none';
            fileZone.style.display = 'inline-block';
        }
    });
    dropZone.addEventListener('drop', async (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-zone');
        dropZone.classList.remove('drag-zone-active');
        const file = e.dataTransfer?.files[0];
        let parts = file.name.split('.');
        if (!file || file.name.split('.').length === 1) {
            document.getElementById('warning').style.visibility = 'visible';
            setTimeout(() => {
                document.getElementById('warning').style.visibility = 'hidden';
                if (otherFiles.length !== 0 || images.length !== 0 && textFiles.length !== 0) {
                    dropZone.style.display = 'none';
                    document.getElementById('files').style.display = 'inline-block';
                }
            }, 2000);
            return;
        }
        else if (allowedImages.has(file.type)) {
            var fileData = { file: file, options: { convertToPdf: false } };
            images.push(fileData);
            addFile(file, "image");
            await saveToStorage(fileData, "image").catch((error) => console.error(error));
        }

        else if (parts.length > 1 && allowedTextFiles.has(parts.pop())) {
            var fileData = { file: file, options: { convertToPdf: false } };
            textFiles.push(fileData);
            addFile(file, "text");
            await saveToStorage(fileData, "text").catch((error) => console.error(error));
        }
        else {
            otherFiles.push(file);
            addFile(file, "other");
            await saveToStorage({ file: file }, "other").catch((error) => console.error(error));
        }
        dropZone.style.display = 'none';
        document.getElementById('files').style.display = 'inline-block';
        

    });
    
    document.getElementById('confirm-options').addEventListener('click', async () => {
        var checked = document.getElementById('convert-to-pdf').checked;
        let arr = ind.split(';');
        let file;
        let type;
        if (parseInt(arr[0]) == 1) {
            file = textFiles[parseInt(arr[1])];
            type = "text";
        }
        else if (parseInt(arr[0]) == 2) {
            images = textFiles[parseInt(arr[1])];
            type = "image";
        }
        ind = "";
        if (file.options.convertToPdf != checked) {
            file.options.convertToPdf = checked;
            updateStorage(type);
        }


        document.getElementById('files').style.display = 'inline-block';
        document.getElementById('options-zone').style.display = 'none';
    });
    fileZone.addEventListener('dragenter', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-zone');
        dropZone.classList.add('drag-zone-active');
        dropZone.style.display = 'inline-block';
        fileZone.style.display = 'none';
    });

    document.getElementById('cancel').addEventListener('click', async () => {
        document.getElementById('files').style.display = 'none';
        dropZone.style.display = 'inline-block';
        document.getElementById('file-list').innerHTML = "";
        await chrome.storage.local.clear();
        
    }); 

    document.getElementById('save').addEventListener('click', async () => {
        const formData = new FormData()
        textFiles.forEach((file, index) => {
            formData.append(`TextFiles[${index}].File`, file.file);
            formData.append(`TextFiles[${index}].Options.ConvertToPdf`, file.options.convertToPdf);
        });
        images.forEach((file, index) => {
            formData.append(`Images[${index}].File`, file);
            formData.append(`Images[${index}].Options.ConvertToPdf`, file.options.convertToPdf);
        });
        otherFiles.forEach((file, index) => {
            formData.append(`OtherFile[${index}]`, file);
        });

        const resp = await fetch(`http://localhost:5091/api/converter/parse`, {
            method: 'POST',
            body: formData
        });
        await chrome.storage.local.clear();
        if (resp.ok) {
            fileZone.style.display = 'none';
            document.getElementById('resp-s').style.display = 'inline-block';
            setTimeout(() => {
                document.getElementById('resp-s').style.display = 'none';
                fileZone.style.display = 'none';
                dropZone.style.display = 'inline-block';
            }, 1000);
        }
        else {
            fileZone.style.display = 'none';
            document.getElementById('resp-b').style.display = 'inline-block';
            document.getElementById('resp-mes').innerHTML = resp.statusText;
        }

    });
    document.getElementById('ok').addEventListener('click', () => {
        document.getElementById('resp-b').style.display = 'none';
        dropZone.style.display = 'inline-block';
        
    });

});

async function saveToStorage(file, type) {
    return new Promise((resolve, reject) => {
        var reader = new FileReader();

        reader.onload = async () => {
            let fileData = undefined;
            if (type === "text" || type === "image") {
                fileData = {
                    name: file.file.name,
                    type: file.file.type,
                    size: file.file.size,
                    data: reader.result.split(',')[1],
                    options: {
                        convertToPdf: file.options.convertToPdf
                    }
                };
            }
            else {
                fileData = {
                    name: file.file.name,
                    type: file.file.type,
                    size: file.file.size,
                    data: reader.result.split(',')[1],

                };
            }
            if (type === "text") {
                let { [STORAGE_TEXT_FILES]: storagedFiles = [] } = await chrome.storage.local.get(STORAGE_TEXT_FILES);
                await chrome.storage.local.set({ [STORAGE_TEXT_FILES]: [...storagedFiles, fileData] });
            }
            else if (type === "image") {
                let { [STORAGE_IMAGES]: storagedFiles = [] } = await chrome.storage.local.get(STORAGE_IMAGES);
                await chrome.storage.local.set({ [STORAGE_IMAGES]: [...storagedFiles, fileData] });
            }
            else {
                let { [STORAGE_OTHER]: storagedFiles = [] } = await chrome.storage.local.get(STORAGE_OTHER);
                await chrome.storage.local.set({ [STORAGE_OTHER]: [...storagedFiles, fileData] });
            }
            
            resolve();
        };
        reader.onerror = (error) => reject(error);
        reader.readAsDataURL(file.file);

    });
}


async function updateStorage(type) {
    if (type === "text") {

        await chrome.storage.local.remove(STORAGE_TEXT_FILES);
        for (let file of textFiles) {
            saveToStorage(file, "text");
        }
    }
    else if (type === "image") {
        await chrome.storage.local.remove(STORAGE_IMAGES);
        for (let file of textFiles) {
            saveToStorage(file, "image");
        }
    }
}



async function loadFilesFromStorage() {
    let { [STORAGE_TEXT_FILES]: storagedText = [] } = await chrome.storage.local.get(STORAGE_TEXT_FILES);
    for (var fileData of storagedText) {
        const byteStr = atob(fileData.data);
        const array = new Uint8Array(byteStr.length);
        for (let i = 0; i < array.length; i++) {
            array[i] = byteStr.charCodeAt(i);
        }
        const file = new File([array], fileData.name, { type: fileData.type });
        textFiles.push({ file: file, options: { convertToPdf: fileData.options.convertToPdf } });
        addFile(file, "text");
    }
    let { [STORAGE_IMAGES]: storagedimages = [] } = await chrome.storage.local.get(STORAGE_IMAGES);
    for (var fileData of storagedimages) {
        const byteStr = atob(fileData.data);
        const array = new Uint8Array(byteStr.length);
        for (let i = 0; i < array.length; i++) {
            array[i] = byteStr.charCodeAt(i);
        }
        const file = new File([array], fileData.name, { type: fileData.type });
        images.push({ file: file, options: { convertToPdf: fileData.options.convertToPdf } });
        addFile(file, "image");
    }
    let { [STORAGE_OTHER]: storragedOther = [] } = await chrome.storage.local.get(STORAGE_OTHER);
    for (var fileData of storragedOther) {
        const byteStr = atob(fileData.data);
        const array = new Uint8Array(byteStr.length);
        for (let i = 0; i < array.length; i++) {
            array[i] = byteStr.charCodeAt(i);
        }
        const file = new File([array], fileData.name, { type: fileData.type });
        otherFiles.push({ file: file });
        addFile(file, "other");
    }
}

function addFile(file, type) {
    const settingIcon = `<svg class="settings-icon" width="20" height="20" viewBox="0 0 24 24" name="settingsbtn">
    <path d="M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 
    15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 
    19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 
    19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 
    14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 
    4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 
    4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 
    4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 
    14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 
    19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z" />
    </svg>`;
    const fileIcon = `<svg class="file-icon" width="20" height="20" viewBox="0 0 24 24">
    <path d = "M13,9V3.5L18.5,9M6,2C4.89,2 4,2.89 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2H6Z"/>
    </svg>`;

    const li = document.createElement('li');
    li.className = 'file-line';
    if (type === "text" || type === "images") {
        li.innerHTML = `${fileIcon}
        <p class="file-name">${file.name}</p>
        ${settingIcon}`;
        li.querySelector('svg[name="settingsbtn"]').addEventListener('click', (e) => {
            document.getElementById('files').style.display = 'none';
            document.getElementById('options-zone').style.display = 'inline-block';
            ind = e.currentTarget.id;
        });
        if (type === "image") {
            let curInd = `2;${images.length - 1}`;
            li.querySelector('svg[name="settingsbtn"]').id = curInd;
        }
        else {
            let curInd = `1;${textFiles.length - 1}`;
            li.querySelector('svg[name="settingsbtn"]').id = curInd;
        }
    }
    else {
        li.innerHTML = `${fileIcon}
        <p class="file-name">${file.name}</p>`;
    }

    
    const ul = document.getElementById('file-list');
    ul.appendChild(li);

}

