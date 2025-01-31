﻿import './search.css'

const Search = (props) => {

    function setSearch(e) {
        props.search(e)
    }


    return (

        <section className="search-bar">

            <div className="search">
                <input type="search-input" placeholder="Search..." onChange={(e) => {
                    setSearch(e.target.value)
                }} />
                <span className="search-icon"> <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M21.2788 18.7212C20.6933 18.1605 20.1235 17.5836 19.57 16.9913C19.105 16.5188 18.825 16.175 18.825 16.175L15.325 14.5037C16.7262 12.9145 17.4996 10.8687 17.5 8.75C17.5 3.92625 13.575 0 8.75 0C3.925 0 0 3.92625 0 8.75C0 13.5737 3.925 17.5 8.75 17.5C10.9537 17.5 12.9625 16.675 14.5037 15.3263L16.175 18.8263C16.175 18.8263 16.5188 19.1062 16.9913 19.5712C17.475 20.025 18.1112 20.6388 18.7212 21.28L20.4187 23.02L21.1737 23.8275L23.825 21.1763L23.0175 20.4212C22.5437 19.9562 21.9113 19.3387 21.2788 18.7212ZM8.75 15C5.30375 15 2.5 12.1962 2.5 8.75C2.5 5.30375 5.30375 2.5 8.75 2.5C12.1962 2.5 15 5.30375 15 8.75C15 12.1962 12.1962 15 8.75 15Z" fill="#545454" />
                </svg></span>
            </div>

        </section>

    )
}

export default Search