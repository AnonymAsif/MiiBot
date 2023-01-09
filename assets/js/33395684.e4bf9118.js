"use strict";(self.webpackChunkmy_website=self.webpackChunkmy_website||[]).push([[5016],{3905:(e,t,n)=>{n.d(t,{Zo:()=>d,kt:()=>v});var r=n(7294);function o(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function a(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?a(Object(n),!0).forEach((function(t){o(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):a(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,r,o=function(e,t){if(null==e)return{};var n,r,o={},a=Object.keys(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||(o[n]=e[n]);return o}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(o[n]=e[n])}return o}var s=r.createContext({}),u=function(e){var t=r.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},d=function(e){var t=u(e.components);return r.createElement(s.Provider,{value:t},e.children)},p="mdxType",m={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},c=r.forwardRef((function(e,t){var n=e.components,o=e.mdxType,a=e.originalType,s=e.parentName,d=l(e,["components","mdxType","originalType","parentName"]),p=u(n),c=o,v=p["".concat(s,".").concat(c)]||p[c]||m[c]||a;return n?r.createElement(v,i(i({ref:t},d),{},{components:n})):r.createElement(v,i({ref:t},d))}));function v(e,t){var n=arguments,o=t&&t.mdxType;if("string"==typeof e||o){var a=n.length,i=new Array(a);i[0]=c;var l={};for(var s in t)hasOwnProperty.call(t,s)&&(l[s]=t[s]);l.originalType=e,l[p]="string"==typeof e?e:o,i[1]=l;for(var u=2;u<a;u++)i[u]=n[u];return r.createElement.apply(null,i)}return r.createElement.apply(null,n)}c.displayName="MDXCreateElement"},1514:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>s,contentTitle:()=>i,default:()=>p,frontMatter:()=>a,metadata:()=>l,toc:()=>u});var r=n(7462),o=(n(7294),n(3905));const a={sidebar_position:5},i="Remove",l={unversionedId:"queue/remove",id:"queue/remove",title:"Remove",description:"Removes a song or range of songs in the current queue (by index).",source:"@site/docs/queue/remove.md",sourceDirName:"queue",slug:"/queue/remove",permalink:"/MiiBot/docs/queue/remove",draft:!1,tags:[],version:"current",sidebarPosition:5,frontMatter:{sidebar_position:5},sidebar:"defaultSidebar",previous:{title:"Move",permalink:"/MiiBot/docs/queue/move"},next:{title:"Save",permalink:"/MiiBot/docs/queue/save"}},s={},u=[{value:"Usage",id:"usage",level:3},{value:"Parameters",id:"parameters",level:3},{value:"Required:",id:"required",level:4},{value:"Optional:",id:"optional",level:4},{value:"Checks",id:"checks",level:3},{value:"Embeds",id:"embeds",level:2}],d={toc:u};function p(e){let{components:t,...n}=e;return(0,o.kt)("wrapper",(0,r.Z)({},d,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"remove"},"Remove"),(0,o.kt)("p",null,"Removes a song or range of songs in the current queue (by index)."),(0,o.kt)("admonition",{type:"note"},(0,o.kt)("p",{parentName:"admonition"},"The currently playing song ",(0,o.kt)("strong",{parentName:"p"},"cannot")," be removed.")),(0,o.kt)("h3",{id:"usage"},"Usage"),(0,o.kt)("p",null,(0,o.kt)("inlineCode",{parentName:"p"},"/queue remove StartIndex:[index]")),(0,o.kt)("p",null,(0,o.kt)("inlineCode",{parentName:"p"},"/queue remove StartIndex:[index] EndIndex:[index]")),(0,o.kt)("h3",{id:"parameters"},"Parameters"),(0,o.kt)("h4",{id:"required"},"Required:"),(0,o.kt)("p",null,(0,o.kt)("inlineCode",{parentName:"p"},"long"),"\n",(0,o.kt)("inlineCode",{parentName:"p"},"StartIndex")," - ","[index]"),(0,o.kt)("p",null,"Gets the original index of the song to remove."),(0,o.kt)("h4",{id:"optional"},"Optional:"),(0,o.kt)("p",null,(0,o.kt)("inlineCode",{parentName:"p"},"long"),"\n",(0,o.kt)("inlineCode",{parentName:"p"},"EndIndex")," - ","[index]"),(0,o.kt)("p",null,"Gets the end of the range of songs to remove."),(0,o.kt)("admonition",{type:"info"},(0,o.kt)("p",{parentName:"admonition"},"The positions of the other songs will be affected by this change.")),(0,o.kt)("h3",{id:"checks"},"Checks"),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("a",{parentName:"li",href:"/docs/Advanced/validation/uservc"},"User Voice Channel"))),(0,o.kt)("h2",{id:"embeds"},"Embeds"),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Songs Removed"',title:'"Songs','Removed"':!0},"Song(s) Removed\n\nYour song(s) have been removed from the queue:\n[Title 1]\n...\n[Title #]\n")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="No Index"',title:'"No','Index"':!0},"No Index Provided\nMiiBot couldn't figure out which song to remove\n")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Empty Queue"',title:'"Empty','Queue"':!0},"Empty Queue\nMiiBot has no songs to remove\n")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Invalid Range"',title:'"Invalid','Range"':!0},"Invalid Range\nMiiBot couldn't get the songs in this range\n")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Invalid Indices"',title:'"Invalid','Indices"':!0},"Invalid Positions\nMiibot couldn't get the song at that position\n")))}p.isMDXComponent=!0}}]);