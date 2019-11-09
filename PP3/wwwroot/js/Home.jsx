class SearchBox extends React.Component {
    render() {
        return (            
            <div className="search-container">
                <textarea>
                    Search Text
                </textarea>
            </div>
        );
    }
}

ReactDOM.render(
    <SearchBox />,
    document.getElementById('content')
);