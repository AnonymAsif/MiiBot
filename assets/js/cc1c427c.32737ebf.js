"use strict";(self.webpackChunkmy_website=self.webpackChunkmy_website||[]).push([[974],{3905:(e,t,n)=>{n.d(t,{Zo:()=>s,kt:()=>y});var a=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function i(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function o(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?i(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):i(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,a,r=function(e,t){if(null==e)return{};var n,a,r={},i=Object.keys(e);for(a=0;a<i.length;a++)n=i[a],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);for(a=0;a<i.length;a++)n=i[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var p=a.createContext({}),u=function(e){var t=a.useContext(p),n=t;return e&&(n="function"==typeof e?e(t):o(o({},t),e)),n},s=function(e){var t=u(e.components);return a.createElement(p.Provider,{value:t},e.children)},d="mdxType",c={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},m=a.forwardRef((function(e,t){var n=e.components,r=e.mdxType,i=e.originalType,p=e.parentName,s=l(e,["components","mdxType","originalType","parentName"]),d=u(n),m=r,y=d["".concat(p,".").concat(m)]||d[m]||c[m]||i;return n?a.createElement(y,o(o({ref:t},s),{},{components:n})):a.createElement(y,o({ref:t},s))}));function y(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var i=n.length,o=new Array(i);o[0]=m;var l={};for(var p in t)hasOwnProperty.call(t,p)&&(l[p]=t[p]);l.originalType=e,l[d]="string"==typeof e?e:r,o[1]=l;for(var u=2;u<i;u++)o[u]=n[u];return a.createElement.apply(null,o)}return a.createElement.apply(null,n)}m.displayName="MDXCreateElement"},1768:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>p,contentTitle:()=>o,default:()=>d,frontMatter:()=>i,metadata:()=>l,toc:()=>u});var a=n(7462),r=(n(7294),n(3905));const i={sidebar_position:3},o="Play",l={unversionedId:"player/play",id:"player/play",title:"Play",description:"Adds a song to the queue.",source:"@site/docs/player/play.md",sourceDirName:"player",slug:"/player/play",permalink:"/MiiBot/docs/player/play",draft:!1,tags:[],version:"current",sidebarPosition:3,frontMatter:{sidebar_position:3},sidebar:"defaultSidebar",previous:{title:"Disconnect",permalink:"/MiiBot/docs/player/disconnect"},next:{title:"Pause",permalink:"/MiiBot/docs/player/pause"}},p={},u=[{value:"Usage",id:"usage",level:3},{value:"Parameters",id:"parameters",level:3},{value:"Required:",id:"required",level:4},{value:"URL Query",id:"url-query",level:5},{value:"Search Query",id:"search-query",level:5},{value:"Optional:",id:"optional",level:4},{value:"Checks",id:"checks",level:3},{value:"Embeds",id:"embeds",level:2}],s={toc:u};function d(e){let{components:t,...n}=e;return(0,r.kt)("wrapper",(0,a.Z)({},s,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"play"},"Play"),(0,r.kt)("p",null,"Adds a song to the queue."),(0,r.kt)("admonition",{type:"note"},(0,r.kt)("p",{parentName:"admonition"},"If the queue is empty the song will play immediately")),(0,r.kt)("h3",{id:"usage"},"Usage"),(0,r.kt)("p",null,(0,r.kt)("inlineCode",{parentName:"p"},"/player play search:[query]")),(0,r.kt)("p",null,(0,r.kt)("inlineCode",{parentName:"p"},"/player play search:[query] platform:[SoundCloud]")),(0,r.kt)("h3",{id:"parameters"},"Parameters"),(0,r.kt)("h4",{id:"required"},"Required:"),(0,r.kt)("p",null,(0,r.kt)("inlineCode",{parentName:"p"},"string"),"\n",(0,r.kt)("inlineCode",{parentName:"p"},"search")," - ","[query]"),(0,r.kt)("p",null,"Gets the users search query."),(0,r.kt)("admonition",{title:"Queries",type:"note"},(0,r.kt)("p",{parentName:"admonition"},"If the query is a valid URL, MiiBot will attempt to play from the URL."),(0,r.kt)("p",{parentName:"admonition"},"Otherwise, MiiBot will search the specified platform for a track with a matching title.")),(0,r.kt)("h5",{id:"url-query"},"URL Query"),(0,r.kt)("admonition",{title:"Playlists",type:"info"},(0,r.kt)("p",{parentName:"admonition"},"If the URL provided is a playlist, MiiBot will add all songs in the playlist to the Queue.")),(0,r.kt)("h5",{id:"search-query"},"Search Query"),(0,r.kt)("p",null,"If the query is not a URL, MiiBot will send an embed with 5 buttons for the user to choose from. "),(0,r.kt)("admonition",{type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"Make sure to respond quickly! MiiBot can time out! See ",(0,r.kt)("a",{parentName:"p",href:"/"},"interactions")," for more info.")),(0,r.kt)("h4",{id:"optional"},"Optional:"),(0,r.kt)("p",null,(0,r.kt)("inlineCode",{parentName:"p"},"Choice"),"\n",(0,r.kt)("inlineCode",{parentName:"p"},"platform")," - Youtube/SoundCloud"),(0,r.kt)("p",null,"Gets the URL to search from."),(0,r.kt)("admonition",{type:"note"},(0,r.kt)("p",{parentName:"admonition"},"The platform will be ignored if the user provides a URL query.")),(0,r.kt)("h3",{id:"checks"},"Checks"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/Advanced/validation/lavalink"},"Lavalink Validation")),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/Advanced/validation/uservc"},"User Voice Channel"))),(0,r.kt)("admonition",{title:"Connect",type:"info"},(0,r.kt)("p",{parentName:"admonition"},"If the user is not already in a voice channel, this command calls the ",(0,r.kt)("a",{parentName:"p",href:"/docs/player/connect"},"connect")," command.")),(0,r.kt)("h2",{id:"embeds"},"Embeds"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Song Queued"',title:'"Song','Queued"':!0},"Song Queued\n\n[Title] has been queued\nPosition: [#]\n")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Song Playing"',title:'"Song','Playing"':!0},"Playing [Title]\n\nBy: [Author]\nLength: [Length]\n")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="No Query"',title:'"No','Query"':!0},"No Search Query Provided\nMiiBot doesn't have any query to work with\n")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="Load Failed"',title:'"Load','Failed"':!0},"Something Went Wrong\nMiiBot couldn't load results\n")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-mdx",metastring:'title="No Matches"',title:'"No','Matches"':!0},"Nothing Found\nMiiBot couldn't gather any results from your search query\n")))}d.isMDXComponent=!0}}]);