"use strict";(self.webpackChunkmy_website=self.webpackChunkmy_website||[]).push([[2938],{3905:(e,t,r)=>{r.d(t,{Zo:()=>u,kt:()=>f});var n=r(7294);function a(e,t,r){return t in e?Object.defineProperty(e,t,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[t]=r,e}function o(e,t){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),r.push.apply(r,n)}return r}function i(e){for(var t=1;t<arguments.length;t++){var r=null!=arguments[t]?arguments[t]:{};t%2?o(Object(r),!0).forEach((function(t){a(e,t,r[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):o(Object(r)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))}))}return e}function l(e,t){if(null==e)return{};var r,n,a=function(e,t){if(null==e)return{};var r,n,a={},o=Object.keys(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||(a[r]=e[r]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(a[r]=e[r])}return a}var p=n.createContext({}),s=function(e){var t=n.useContext(p),r=t;return e&&(r="function"==typeof e?e(t):i(i({},t),e)),r},u=function(e){var t=s(e.components);return n.createElement(p.Provider,{value:t},e.children)},c="mdxType",d={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},m=n.forwardRef((function(e,t){var r=e.components,a=e.mdxType,o=e.originalType,p=e.parentName,u=l(e,["components","mdxType","originalType","parentName"]),c=s(r),m=a,f=c["".concat(p,".").concat(m)]||c[m]||d[m]||o;return r?n.createElement(f,i(i({ref:t},u),{},{components:r})):n.createElement(f,i({ref:t},u))}));function f(e,t){var r=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var o=r.length,i=new Array(o);i[0]=m;var l={};for(var p in t)hasOwnProperty.call(t,p)&&(l[p]=t[p]);l.originalType=e,l[c]="string"==typeof e?e:a,i[1]=l;for(var s=2;s<o;s++)i[s]=r[s];return n.createElement.apply(null,i)}return n.createElement.apply(null,r)}m.displayName="MDXCreateElement"},5330:(e,t,r)=>{r.r(t),r.d(t,{assets:()=>p,contentTitle:()=>i,default:()=>c,frontMatter:()=>o,metadata:()=>l,toc:()=>s});var n=r(7462),a=(r(7294),r(3905));const o={sidebar_position:7},i="State Variables",l={unversionedId:"states",id:"states",title:"State Variables",description:"MiiBot has three* state Variables:",source:"@site/docs/states.md",sourceDirName:".",slug:"/states",permalink:"/MiiBot/docs/states",draft:!1,tags:[],version:"current",sidebarPosition:7,frontMatter:{sidebar_position:7},sidebar:"defaultSidebar",previous:{title:"Help",permalink:"/MiiBot/docs/info/help"},next:{title:"Command Validation",permalink:"/MiiBot/docs/category/command-validation"}},p={},s=[{value:"Descriptions",id:"descriptions",level:2},{value:"Song Loop",id:"song-loop",level:3},{value:"Queue Loop",id:"queue-loop",level:3},{value:"Paused",id:"paused",level:3}],u={toc:s};function c(e){let{components:t,...r}=e;return(0,a.kt)("wrapper",(0,n.Z)({},u,r,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("h1",{id:"state-variables"},"State Variables"),(0,a.kt)("p",null,"MiiBot has three* state Variables:"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},"Song Loop"),(0,a.kt)("li",{parentName:"ul"},"Queue Loop"),(0,a.kt)("li",{parentName:"ul"},"Paused")),(0,a.kt)("p",null,"*Developer Note: There is a fourth state variable, ",(0,a.kt)("inlineCode",{parentName:"p"},"skipping")," which cannot be toggled by the user."),(0,a.kt)("h2",{id:"descriptions"},"Descriptions"),(0,a.kt)("h3",{id:"song-loop"},"Song Loop"),(0,a.kt)("p",null,"If enabled, the current song will repeat. Can be emabled or disabled with the ",(0,a.kt)("a",{parentName:"p",href:"/docs/player/loop"},"loop")," command."),(0,a.kt)("h3",{id:"queue-loop"},"Queue Loop"),(0,a.kt)("p",null,"If enabled, the current queue will repeat. Can be enabled or disabled with the ",(0,a.kt)("a",{parentName:"p",href:"/docs/player/loop"},"loop")," command."),(0,a.kt)("h3",{id:"paused"},"Paused"),(0,a.kt)("p",null,"If enabled, the current playing song will be paused. Can be enabled with the ",(0,a.kt)("a",{parentName:"p",href:"/docs/player/pause"},"pause")," command, or disabled with the ",(0,a.kt)("a",{parentName:"p",href:"/docs/player/resume"},"resume")," command."),(0,a.kt)("admonition",{title:"Both Loops",type:"info"},(0,a.kt)("p",{parentName:"admonition"},"If both song and queue loops are enabled, the current song will repeat until skipped. However skipping will move the song to the back of the queue. See ",(0,a.kt)("a",{parentName:"p",href:"/docs/player/skip"},"skip")," for more details.")))}c.isMDXComponent=!0}}]);