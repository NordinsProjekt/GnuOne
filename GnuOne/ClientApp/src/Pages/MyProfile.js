﻿import React, { useState, useEffect, useContext } from 'react';
import MeContext from '../contexts/meContext'
import PortContext from '../contexts/portContext'
import ProfileWheel from '../components/MenuWheel/ProfileWheel';
import "./MyProfile.min.css";
import FriendContext from '../contexts/friendContext'
import Navbar from '../components/Navbar/NavBar';

//testar
function MyProfile({ routes }) {
    const port = useContext(PortContext)
    const url = `https://localhost:${port}/api/settings/`
    const [myEmail, setMyEmail] = useState('')
  

    useEffect(() => {
        async function fetchData() {
            console.log('fetching')
            const response = await fetch(url)
            const me = await response.json()
            const myEmail = me.email
            console.log(myEmail)
            setMyEmail(myEmail);
        }
        fetchData()
    }, [setMyEmail, url])

   
    return (
        <MeContext.Provider value={myEmail}>
            <FriendContext.Provider value={{ friendEmail: undefined, friendImg: undefined }}>

                <main className="main">
                    <Navbar />
                    <ProfileWheel routes={routes} />
                </main>
            </FriendContext.Provider>

        </MeContext.Provider>
    )
}

export default MyProfile;