console.log("WinUIEdge is injecting");
let styleAdded = false;
const observer = new MutationObserver(function(mutationsList) {
    for (let mutation of mutationsList) {
        if (mutation.type === "childList") {
            const addedNodes = mutation.addedNodes;
            for (let node of addedNodes) {
                findAndReplaceInstallButtons(node);
            }
        }
    }
});

const rootElement = document.getElementById("root");
if(rootElement) {
    findAndReplaceInstallButtons(rootElement);
    observer.observe(document.getElementById("root"), {childList: true, subtree: true, attributes: true, characterData: true});
}

function findAndReplaceInstallButtons(element) {
    const originalInstallButtons = findInstallButtons(element);
    if (originalInstallButtons.length === 0) {
        return;
    }
    for (let originalInstallButton of originalInstallButtons) {
        if (!styleAdded) {
            addStyle();
            styleAdded = true;
        }
        replaceButton(originalInstallButton);
    }
}

function findInstallButtons(element) {
    const result = [];
    if (element.children && element.children.length > 0) {
        for (let child of element.children) {
            if (child.id && child.id.startsWith("installButton-") && !child.classList.contains("replacedInstallButton")) {
                result.push(child);
                continue;
            }
            result.push(...findInstallButtons(child));
        }
    }
    return result;
}

function replaceButton(originalInstallButton){
    const installAnchorElement = document.createElement("a");
    installAnchorElement.href = `https://edge.microsoft.com/extensionwebstorebase/v1/crx?response=redirect&x=id%3D${originalInstallButton.id.slice(14)}%26installsource%3Dondemand`;
    let innerText = originalInstallButton.firstElementChild?.innerText?.trim();
    if(!innerText || innerText.length === 0){
        innerText = "获取";
    }
    installAnchorElement.innerText = innerText;
    installAnchorElement.download;
    originalInstallButton.classList.add("replacedInstallButton");
    originalInstallButton.removeChild(originalInstallButton.firstElementChild);
    originalInstallButton.appendChild(installAnchorElement);
}

function addStyle(){
    const styleElement = document.createElement("style");
    styleElement.innerText = `.replacedInstallButton {
    background-color: var(--colorBrandBackground);
    cursor: pointer;
    padding: 0;
    &>a {
        color: var(--colorNeutralForegroundOnBrand);
        display: block;
        padding: 5px var(--spacingHorizontalM);
        text-decoration: none;
    }
    &:hover {
        background-color: var(--colorBrandBackgroundHover);
        cursor: pointer;
    }
}
.incompatible {
    display: none;
}`;
    document.head.appendChild(styleElement);
}