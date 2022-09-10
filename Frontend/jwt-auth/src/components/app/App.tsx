import React, { FC } from 'react'
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom'
import AuthorizedUser from '../authorized-user/AuthorizedUser'
import Login from '../login/Login'
import NotFound from '../pages/NotFound'
import Registration from '../registration/Registration'

const App: FC = () => {
    return (
        <div>
            <Router>
                <Routes>
                    <Route path="/" element={<Registration />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/authorized_user" element={<AuthorizedUser />} />
                    <Route path="/*" element={<NotFound />} />
                </Routes>
            </Router>
        </div>
    )
}

export default App
